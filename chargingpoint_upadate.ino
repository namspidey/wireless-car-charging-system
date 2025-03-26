#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <SPI.h>
#include <RF24.h>

#define SENSOR_SLOT_PIN 2

LiquidCrystal_I2C lcd(0x27, 16, 2);

RF24 radio(9, 10);
const byte address[6] = "00001";

String vehicleLicensePlate = "";
bool vehicleLicensePlateReceived = false;

bool isCharging = false;

float voltage = 0.0;
float current = 0.0;
float temperature = 0.0;
int hours = 0, minutes = 0, seconds = 0;

void setup() {
  lcd.begin(16, 2);
  lcd.clear();
  lcd.backlight();
  lcd.init();

  lcd.setCursor(0, 0);
  lcd.print("W e l c o m e !");
  lcd.setCursor(0, 1);
  lcd.print("W-eV Station 1st ");
  delay(4000);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Initializing...");
  delay(2000);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("System Check...");
  delay(2000);

  radio.begin();
  radio.openReadingPipe(1, address);
  radio.setPALevel(RF24_PA_HIGH);
  radio.setChannel(0x4c);
  radio.startListening();

  pinMode(3, OUTPUT);

  Serial.begin(9600);
}

bool detectObstacleDigital(int sensorDigitalPin) {
  int sensorState = digitalRead(sensorDigitalPin);
  return sensorState == LOW;
}

float readCurrent(int current_pin) {
  float totalVoltage = 0.0;
  int sensorValue;
  float voltage;

  for (int i = 0; i < 200; i++) {
    sensorValue = analogRead(current_pin);
    voltage = sensorValue * (5.0 / 1023.0);
    totalVoltage += voltage;
    delay(5);
  }
  float averageVoltage = totalVoltage / 200.0;
  float current = (averageVoltage - 2.5) / 0.100;

  return current;
}

void relayOn() {
  digitalWrite(3, HIGH);
}

void relayOff() {
  digitalWrite(3, LOW);
}

void system_ready() {
  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("System Status: ");
  lcd.setCursor(0, 1);
  lcd.print("Ready !");
  delay(3800);
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

String receiveVehicleLicensePlate() {
  char receivedText[32] = "";
  if (radio.available()) {
    radio.read(&receivedText, sizeof(receivedText));
    String receivedString = String(receivedText);
    if (receivedString.indexOf("[licensePlate]") != -1) {
      Serial.println(receivedString);
      return receivedString;
    }
  }
  return "";
}

void receiveAndProcessData() {
  if (radio.available()) {
    char receivedText[128] = "";
    radio.read(&receivedText, sizeof(receivedText));

    String fullReceivedData = String(receivedText);

    int idx = 0;
    float values[4];

    while (fullReceivedData.length() > 0 && idx < 4) {
      int commaIndex = fullReceivedData.indexOf(',');
      if (commaIndex == -1) {
        values[idx] = fullReceivedData.toFloat();
        break;
      } else {
        String valueStr = fullReceivedData.substring(0, commaIndex);
        values[idx] = valueStr.toFloat();
        fullReceivedData = fullReceivedData.substring(commaIndex + 1);
        idx++;
      }
    }
    voltage = values[0];
    current = values[1];
    temperature = values[2];

    String timeString = fullReceivedData;
    int hourSeparator = timeString.indexOf(':');
    int minuteSeparator = timeString.indexOf(':', hourSeparator + 1);

    if (hourSeparator != -1 && minuteSeparator != -1) {
      hours = timeString.substring(0, hourSeparator).toInt();
      minutes = timeString.substring(hourSeparator + 1, minuteSeparator).toInt();
      seconds = timeString.substring(minuteSeparator + 1).toInt();
    }

    float totalHours = hours + (minutes / 60.0) + (seconds / 3600.0);
    float power = voltage * current;
    float full_power = 5 * readCurrent(A0);
    float totalEnergy = power * totalHours;
    float battery_percent = voltageToPercentage(voltage);
    float cost = totalEnergy * 4;

    Serial.println("[charging] ChrgPoint ID: " + String(1) + " ");
    Serial.println("[charging] ChrgPoint ID: " + String(1) + " ");
    Serial.println("[charging] Battery Percentage: " + String(battery_percent) + "%");
    Serial.println("[charging] Voltage: " + String(voltage) + "V");
    Serial.println("[charging] Current: " + String(current) + "A");
    Serial.println("[charging] Temperature: " + String(temperature) + "C");
    Serial.println("[charging] Power: " + String(power) + "W");
    Serial.println("[charging] FullErg: " + String(full_power) + "W");
    Serial.println("[charging] Time: " + String(hours) + ":" + String(minutes) + ":" + String(seconds));
    Serial.println("[charging] Energy Consumed: " + String(totalEnergy) + " Wh");
    Serial.println("[charging] Cost: " + String(cost) + " VND");
  }
}

void loop() {
  bool detectCar = detectObstacleDigital(SENSOR_SLOT_PIN);
  float point_current = readCurrent(A0);

  if (detectCar) {
    if (point_current >= 0.25) {
      lcd.clear();
      lcd.setCursor(0, 0);
      lcd.print("System Status: ");
      lcd.setCursor(0, 1);
      lcd.print("Charging ! ");

      isCharging = true;
      receiveAndProcessData();

    } else if (isCharging && point_current < 0.25) {
      system_ready();
      isCharging = false;
      Serial.println("[disconnected]");
      delay(1000);
    } else {
      vehicleLicensePlate = receiveVehicleLicensePlate();
      if (vehicleLicensePlate != "") {
        Serial.println(vehicleLicensePlate);
        String receivedData = Serial.readString();
        int separatorIndex = receivedData.indexOf('-');
        if (separatorIndex != -1) {
          String name = receivedData.substring(0, separatorIndex);
          String balanceStr = receivedData.substring(separatorIndex + 1);
          int balance = balanceStr.toInt();

          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print("Welcome " + name + "!");
          lcd.setCursor(0, 1);
          lcd.print("Balance: " + String(balance) + "VND");
          relayOn();
        }
      }
    }
  } else {
    system_ready();
    isCharging = false;
    Serial.println("[disconnected]");
    delay(1000);
  }
}
