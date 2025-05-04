#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <SPI.h>
#include <RF24.h>
#include <DHT.h>
#include <Keypad.h>

#define DHTPIN 7
#define DHTTYPE DHT11
#define BUZZER_PIN 8

DHT dht(DHTPIN, DHTTYPE);

int noteDuration[] = {
  150
};
int noteDurationForError[] = {
  3000
};
int noteDurationForDone[] = {
  500
};
int noteDurationForBeep[] = {
  50
};
bool isBuzzerPlayed = false;
bool isBuzzerPlayedForError = false;
bool isBuzzerPlayedForFullyCharge = false;
bool isBuzzerPlayedForFullyChargeByPercent = false;
bool isBuzzerPlayedForFullyChargeByAmount = false;

LiquidCrystal_I2C lcd(0x27, 16, 2);

RF24 radio(9, 10);
const byte address[6] = "00001";
String licensePlate = "30K-567.89";

const byte ROW_NUM = 4;
const byte COLUMN_NUM = 4;

char keys[ROW_NUM][COLUMN_NUM] = {
  { '1', '2', '3', 'A' },
  { '4', '5', '6', 'B' },
  { '7', '8', '9', 'C' },
  { '*', '0', '#', 'D' }
};

byte pin_rows[ROW_NUM] = { 22, 23, 24, 25 };
byte pin_column[COLUMN_NUM] = { 26, 27, 28, 29 };

Keypad keypad = Keypad(makeKeymap(keys), pin_rows, pin_column, ROW_NUM, COLUMN_NUM);
char key = '\0';

float chargeAmount;
int chargePercentage;

unsigned long startTime = 0;
float totalEnergy = 0;
float cost = 0;
bool isChargingNow = false;
bool previousChargingStatus = false;
bool isChargingFully = false;
bool isChargingFullyByPercent = false;
bool isChargingFullyByAmount = false;
bool isOverCurrentPreviousCharging = false;
bool isOverTemperaturePreviousCharging = false;

int target_percentage = 0;

void setup() {
  lcd.begin(16, 2);
  lcd.clear();
  lcd.backlight();

  lcd.setCursor(0, 0);
  lcd.print("W e l c o m e");
  lcd.setCursor(0, 1);
  lcd.print("W-eV Vehicle.");
  delay(5000);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("License Plate");
  lcd.setCursor(0, 1);
  lcd.print(licensePlate);
  delay(5000);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Initializing...");
  delay(5000);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("System Check...");
  delay(5000);

  pinMode(11, OUTPUT);

  radio.begin();
  radio.openWritingPipe(address);
  radio.setPALevel(RF24_PA_HIGH);
  radio.setChannel(0x4c);

  dht.begin();
  Serial.begin(9600);
}

float readVoltage(int voltage_pin) {
  float totalVoltage = 0.0;
  int sensorValue;

  for (int i = 0; i < 200; i++) {
    sensorValue = analogRead(voltage_pin);
    float voltage = sensorValue * (25.0 / 1023.0);
    totalVoltage += voltage;
  }
  float averageVoltage = totalVoltage / 200.0;
  return averageVoltage;
}

float voltageToPercentage(float voltage) {
  float percentage;

  if (voltage >= 4.2) {
    percentage = 100;
  } else if (voltage <= 3.0) {
    percentage = 0;
  } else {
    float calculatedPercentage = (voltage - 3.0) * (100.0 / (4.2 - 3.0));
    calculatedPercentage = round(calculatedPercentage * 10.0) / 10.0;
    String percentageStr = String(calculatedPercentage, 1);
    percentage = percentageStr.toFloat();
  }
  return percentage;
}

float readCurrent(int current_pin) {
  float totalCurrent = 0.0;
  int sensorValue;
  float voltage, current;

  for (int i = 0; i < 200; i++) {
    sensorValue = analogRead(current_pin);
    voltage = sensorValue * (5.0 / 1023.0);
    current = (voltage - 2.56) / 0.100;
    totalCurrent += current;
  }
  float averageCurrent = totalCurrent / 200.0;
  return averageCurrent;
}

float readTemperature() {
  float totalTemperature = 0.0;
  int validReadings = 0;

  for (int i = 0; i < 200; i++) {
    float temperature = dht.readTemperature();
    if (!isnan(temperature)) {
      totalTemperature += temperature;
      validReadings++;
    }
  }

  if (validReadings == 0) {
    return -1;
  }

  float averageTemperature = totalTemperature / validReadings;
  return averageTemperature;
}

void relayOn() {
  digitalWrite(11, HIGH);
}

void relayOff() {
  digitalWrite(11, LOW);
}

void sendVehicleLicensePlate() {
  char idToSend[64];
  snprintf(idToSend, sizeof(idToSend), "{[licensePlate] %s}", licensePlate.c_str());
  radio.write(&idToSend, sizeof(idToSend));
}


void successSystemCheckSound() {
  pinMode(BUZZER_PIN, OUTPUT);
  for (int i = 0; i < 1; i++) {
    digitalWrite(BUZZER_PIN, LOW);
    delay(noteDuration[i] / 2);
    digitalWrite(BUZZER_PIN, HIGH);
    delay(noteDuration[i] / 2);
  }
}

void doneSystemSound() {
  pinMode(BUZZER_PIN, OUTPUT);
  for (int i = 0; i < 5; i++) {
    digitalWrite(BUZZER_PIN, LOW);
    delay(noteDurationForDone[i] / 2);
    digitalWrite(BUZZER_PIN, HIGH);
    delay(noteDurationForDone[i] / 2);
  }
}

void errorSystemSound() {
  pinMode(BUZZER_PIN, OUTPUT);
  for (int i = 0; i < 3; i++) {
    digitalWrite(BUZZER_PIN, LOW);
    delay(noteDurationForError[i] / 2);
    digitalWrite(BUZZER_PIN, HIGH);
    delay(noteDurationForError[i] / 2);
  }
}

void beepSystemSound() {
  pinMode(BUZZER_PIN, OUTPUT);
  for (int i = 0; i < 1; i++) {
    digitalWrite(BUZZER_PIN, LOW);
    delay(noteDurationForBeep[i] / 2);
    digitalWrite(BUZZER_PIN, HIGH);
    delay(noteDurationForBeep[i] / 2);
  }
}

void system_ready(float battery_percentage, float temperature) {
  if (!isBuzzerPlayed) {
    successSystemCheckSound();
    isBuzzerPlayed = true;
  }
  relayOff();
  sendVehicleLicensePlate();

  lcd.setCursor(0, 0);
  lcd.print("System Status: ");
  lcd.setCursor(0, 1);
  lcd.print("Ready !");
  delay(3800);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Battery Percent: ");
  lcd.setCursor(0, 1);
  lcd.print(battery_percentage);
  lcd.print(" %");
  delay(1800);

  sendVehicleLicensePlate();

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Temperature: ");
  lcd.setCursor(0, 1);
  lcd.print(temperature, 1);
  lcd.print(" C");
  delay(1800);
}

void system_error(float voltage, float temperature) {
  relayOff();

  lcd.clear();
  lcd.setCursor(0, 0);
  if (voltage <= 3.2) {
    lcd.print("Low Battery!");
  }
  if (temperature >= 50) {
    lcd.print("OverTemperature!");
    lcd.setCursor(0, 1);
    lcd.print("Shut down now.");
  }

  if (!isBuzzerPlayedForError) {
    errorSystemSound();
    isBuzzerPlayed = true;
  }
  delay(1800);
}

bool isCharging(int chargingCheck_pin) {
  int sensorValue = analogRead(chargingCheck_pin);
  float voltage = sensorValue * (25.0 / 1023.0);
  if (voltage >= 3.0) {
    return true;
  } else {
    return false;
  }
}

void charging(float voltage, float current, float temperature, float battery_percentage) {
  float power = voltage * current;

  if (startTime == 0) {
    startTime = millis();
  }

  unsigned long elapsedTimeMillis = millis() - startTime;
  unsigned long elapsedTimeSec = elapsedTimeMillis / 1000;
  unsigned long hours = elapsedTimeSec / 3600;
  unsigned long minutes = (elapsedTimeSec % 3600) / 60;
  unsigned long seconds = elapsedTimeSec % 60;

  totalEnergy = power * (elapsedTimeMillis / 3600000.0);
  float energyInKWh = totalEnergy / 1000.0;
  cost = energyInKWh * 4000.0;

  isChargingNow = true;

  lcd.setCursor(0, 0);
  lcd.print("Battery Percent: ");
  lcd.setCursor(0, 1);
  lcd.print(battery_percentage);
  lcd.print(" %");
  delay(1800);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Battery Voltage: ");
  lcd.setCursor(0, 1);
  lcd.print(voltage);
  lcd.print(" V");
  delay(1800);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Charging Current: ");
  lcd.setCursor(0, 1);
  lcd.print(current);
  lcd.print(" A");
  delay(1800);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Temperature: ");
  lcd.setCursor(0, 1);
  lcd.print(temperature, 1);
  lcd.print(" C");
  delay(1800);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Charging Power: ");
  lcd.setCursor(0, 1);
  lcd.print(power);
  lcd.print(" W");
  delay(1800);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Charging Time: ");
  lcd.setCursor(0, 1);
  lcd.print(hours);
  lcd.print("h ");
  lcd.print(minutes);
  lcd.print("m ");
  lcd.print(seconds);
  lcd.print("s");
  delay(1800);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Total Energy: ");
  lcd.setCursor(0, 1);
  lcd.print(totalEnergy, 3);
  lcd.print(" Wh");
  delay(1800);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Total Cost: ");
  lcd.setCursor(0, 1);
  lcd.print(cost, 3);
  lcd.print(" VND");
  delay(1800);

  String timeString = String(hours) + ":" + String(minutes) + ":" + String(seconds);
  sendData(voltage, current, temperature, timeString);

  delay(2000);
}

void sendData(float voltage, float current, float temperature, String timeString) {
  String data = String(voltage) + "," + String(current) + "," + String(temperature) + "," + timeString;
  const char* dataToSend = data.c_str();
  radio.write(dataToSend, strlen(dataToSend));

  Serial.println(data);
}

bool isOverChargingTemperature(float temperature) {
  if (temperature >= 45) {
    isOverTemperaturePreviousCharging = true;
    return true;
  } else {
    isOverTemperaturePreviousCharging = false;
    return false;
  }
}

bool isOverChargingCurrent(float current) {
  if (current > 5) {
    isOverCurrentPreviousCharging = true;
  } else {
    isOverCurrentPreviousCharging = false;
    return false;
  }
}

bool isFullyCharge(float voltage) {
  if (voltage >= 4.2) {
    isChargingFully = true;
    return true;
  } else {
    return false;
  }
}

bool isFullyChargeByPercent(int batteryPercent) {
  if (batteryPercent >= target_percentage) {
    return true;
  } else {
    return false;
  }
}

bool isFullyChargeByAmount(float amountToCharge, float amountChargingSession) {
  if (amountChargingSession >= amountToCharge) {
    return true;
  } else {
    return false;
  }
}

void OverChargingTemperature() {
  isBuzzerPlayedForError = false;

  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("OverTemperature.");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;

  if (!isBuzzerPlayedForError) {
    errorSystemSound();
    isBuzzerPlayedForError = true;
  }
}

void OverChargingCurrent() {
  isBuzzerPlayedForError = false;

  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("OverCurrent.");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;

  if (!isBuzzerPlayedForError) {
    errorSystemSound();
    isBuzzerPlayedForError = true;
  }
}

void fullyCharge() {
  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("ChargingDone.");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;

  if (!isBuzzerPlayedForFullyCharge) {
    doneSystemSound();
    isBuzzerPlayedForFullyCharge = true;
  }
}

void fullyChargeByPercent(int target_percentage) {
  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Achieved " + String(target_percentage) + "%");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;

  if (!isBuzzerPlayedForFullyChargeByPercent) {
    doneSystemSound();
    isBuzzerPlayedForFullyChargeByPercent = true;
  }
}

void fullyChargeByAmount(float amount) {
  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Achieved " + String(amount) + " VND.");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;

  if (!isBuzzerPlayedForFullyChargeByAmount) {
    doneSystemSound();
    isBuzzerPlayedForFullyChargeByAmount = true;
  }
}

void displayChargingOptions() {
  lcd.setCursor(0, 0);
  lcd.print("A:Limit Percent");
  lcd.setCursor(0, 1);
  lcd.print("B:Limit Amount");
  delay(1000);
}

int enterPercentage() {
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Enter%ToCharge:");
  char key;
  String enteredPercentage = "";

  while (true) {
    key = keypad.getKey();
    if (key) {
      if (key >= '0' && key <= '9') {

        String temp = enteredPercentage + key;
        int value = temp.toInt();
        if (value <= 100) {
          enteredPercentage = temp;
          lcd.setCursor(0, 1);
          lcd.print("Charging: ");
          lcd.print(enteredPercentage + "%");
          beepSystemSound();
        } else {
          lcd.clear();
          beepSystemSound();
          lcd.setCursor(0, 1);
          lcd.print("Max is 100%");
          delay(1000);
          lcd.setCursor(0, 1);
          lcd.print("Charging: ");
          lcd.print(enteredPercentage + "%");
        }
      }

      if (key == '#') {
        if (enteredPercentage != "") {
          chargePercentage = enteredPercentage.toInt();
          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print("Charging to ");
          lcd.print(chargePercentage);
          lcd.print("%");
          beepSystemSound();
          delay(2000);
          break;
        } else {
          beepSystemSound();
          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print("Please fill in!");
          delay(1000);
          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print("Enter%ToCharge:");
        }
      }
      if (key == '*') {
        enteredPercentage = "";
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Enter%tocharge:");
        beepSystemSound();
      }
    }
  }
  return chargePercentage;
}

float enterAmount() {
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Enter $ to charge:");
  char key;
  String enteredAmount = "";

  bool firstDigitZero = false;

  while (true) {
    key = keypad.getKey();
    if (key) {
      if (key >= '0' && key <= '9') {
        if (enteredAmount == "" && key == '0') {
          continue;
        }

        enteredAmount += key;
        lcd.setCursor(0, 1);
        lcd.print("Charging: $");
        lcd.print(enteredAmount);
        beepSystemSound();
      }

      if (key == '#') {
        if (enteredAmount != "") {
          chargeAmount = enteredAmount.toFloat();
          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print("Charging to $");
          lcd.print(chargeAmount);
          beepSystemSound();
          delay(2000);
          break;
        } else {
          beepSystemSound();
          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print("Invalid Input!");
          delay(1000);
          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print("Enter $ to charge:");
        }
      }
      if (key == '*') {
        enteredAmount = "";
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Enter $ to charge:");
        beepSystemSound();
      }
    }
  }
  return chargeAmount;
}


void loop() {
  bool chargingStatus = isCharging(A2);
  float voltage = readVoltage(A0);
  float current = readCurrent(A8);
  float temperature = readTemperature();
  float battery_percentage = voltageToPercentage(voltage);
  bool isFullyCharged = isFullyCharge(voltage);
  bool isOverChargedCurrent = isOverChargingCurrent(current);
  bool isOverChargedTemperature = isOverChargingTemperature(temperature);

  if (chargingStatus != previousChargingStatus) {
    if (!chargingStatus) {
      startTime = 0;
      totalEnergy = 0;
    }
    previousChargingStatus = chargingStatus;
  }
  lcd.clear();

  if (chargingStatus == true) {
    if (isFullyCharged == true || isChargingFully == true) {
      fullyCharge();
    } else if (isOverChargedCurrent == true || isOverCurrentPreviousCharging == true) {
      OverChargingCurrent();
    } else if (isOverChargedTemperature == true || isOverTemperaturePreviousCharging == true) {
      OverChargingTemperature();
    } else if (isChargingNow == false && isChargingFullyByPercent == false) {
      displayChargingOptions();
    } else if (isChargingNow == true && isChargingFullyByPercent == false) {
      if (!(isFullyChargeByPercent(battery_percentage))) {
        relayOn();
        charging(voltage, current, temperature, battery_percentage);
      } else {
        fullyChargeByPercent(chargePercentage);
        isChargingFullyByPercent = true;
        relayOff();
        isChargingNow = false;
        isChargingFullyByPercent = true;
      }
    } else if (isChargingNow == false && isChargingFullyByPercent == true) {
      fullyChargeByPercent(target_percentage);
    }

    key = keypad.getKey();
    if (key == 'A') {
      beepSystemSound();
      target_percentage = enterPercentage();
      if (!(isFullyChargeByPercent(battery_percentage))) {
        relayOn();
        charging(voltage, current, temperature, battery_percentage);
      } else {
        fullyChargeByPercent(chargePercentage);
        isChargingFullyByPercent = true;
        relayOff();
      }
    }

  } else {
    isChargingFullyByPercent = false;
    isChargingNow = false;
    isBuzzerPlayedForError = false;
    isBuzzerPlayedForFullyCharge = false;
    isBuzzerPlayedForFullyChargeByPercent = false;
    isBuzzerPlayedForFullyChargeByAmount = false;
    isOverCurrentPreviousCharging = false;
    isOverTemperaturePreviousCharging = false;
    if (voltage > 3.2 && temperature < 50) {
      system_ready(battery_percentage, temperature);
    } else {
      system_error(voltage, temperature);
    }
    delay(1000);
  }
}
