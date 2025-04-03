#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <SPI.h>
#include <RF24.h>
#include <DHT.h>
#include <Keypad.h>

#define DHTPIN 7
#define DHTTYPE DHT11

DHT dht(DHTPIN, DHTTYPE);

LiquidCrystal_I2C lcd(0x27, 16, 2);

RF24 radio(9, 10);
const byte address[6] = "00001";
String licensePlate = "30K56789";

// Khai báo mảng bàn phím
const byte ROW_NUM = 4;
const byte COLUMN_NUM = 4;

// Bảng các phím của bàn phím
char keys[ROW_NUM][COLUMN_NUM] = {
  { '1', '2', '3', 'A' },
  { '4', '5', '6', 'B' },
  { '7', '8', '9', 'C' },
  { '*', '0', '#', 'D' }
};

// Mảng các chân kết nối với Arduino Mega
byte pin_rows[ROW_NUM] = { 22, 23, 24, 25 };
byte pin_column[COLUMN_NUM] = { 26, 27, 28, 29 };

// Khởi tạo đối tượng bàn phím
Keypad keypad = Keypad(makeKeymap(keys), pin_rows, pin_column, ROW_NUM, COLUMN_NUM);

// Biến lưu giá trị phần trăm và giá tiền
float chargeAmount;
int chargePercentage;

unsigned long startTime = 0;
float totalEnergy = 0;
bool isChargingNow = false;
bool previousChargingStatus = false;
bool isChargingFully = false;
bool isChargingFullyByPercent = false;
bool isChargingFullyByAmount = false;
bool isOverCurrentPreviousCharging = false;
bool isOverTemperaturePreviousCharging = false;

void setup() {
  lcd.begin(16, 2);
  lcd.clear();
  lcd.backlight();


  lcd.setCursor(0, 0);
  lcd.print("Welcome VanAnh");
  lcd.setCursor(0, 1);
  lcd.print("Porsche Panamera");
  delay(2000);


  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Initializing...");
  delay(2000);


  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("System Check...");
  delay(2000);


  pinMode(11, OUTPUT);
  radio.begin();
  radio.openWritingPipe(address);
  radio.setPALevel(RF24_PA_HIGH);
  radio.setChannel(0x4c);
  radio.startListening();

  dht.begin();
  Serial.begin(9600);
}

float readVoltage(int voltage_pin) {
  int sensorValue = analogRead(voltage_pin);
  float voltage = sensorValue * (25.0 / 1023.0);
  return voltage;
}

float voltageToPercentage(float voltage) {
  if (voltage >= 4.2) {
    return 100.0;
  } else if (voltage <= 3.0) {
    return 0.0;
  } else {
    return (voltage - 3.0) * (100.0 / (4.2 - 3.0));
  }
}

float readCurrent(int current_pin) {
  int sensorValue = analogRead(current_pin);
  float voltage = sensorValue * (5.0 / 1023.0);
  float current = (voltage - 2.5) / 0.185 - 0.05;
  return current;
}

float readTemperature() {
  float temperature = dht.readTemperature();
  if (isnan(temperature)) {
    return -1;
  }
  return temperature;  // Trả về giá trị nhiệt độ
}

void relayOn() {
  digitalWrite(11, HIGH);
}

void relayOff() {
  digitalWrite(11, LOW);
}

void sendVehicleLicensePlate() {
  char idToSend[32];
  strcpy(idToSend, licensePlate.c_str());

  Serial.print("Sending vehicle license plate: ");
  Serial.println(licensePlate.c_str());  // In ra biển số xe trên Serial Monitor

  bool success = radio.write(&idToSend, sizeof(idToSend));  // Gửi dữ liệu qua RF24
  if (success) {
    Serial.println("License plate sent successfully!");
  } else {
    Serial.println("License plate send failed.");
  }
}

void system_ready(float battery_percentage, float temperature) {
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
    lcd.print("Voltage Error!");
  }
  if (temperature >= 50) {
    lcd.print("Temp Error!");
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

  totalEnergy = power * (elapsedTimeMillis / 3600000.0);  // Wh = Power (W) * Time (h)

  relayOn();
  isChargingNow = true;

  lcd.setCursor(0, 0);
  lcd.print("System Status: ");
  lcd.setCursor(0, 1);
  lcd.print("Charging!");
  delay(3800);

  lcd.clear();
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
}

bool isOverChargingTemperature(float temperature) {
  if (temperature >= 50) {
    isOverTemperaturePreviousCharging = true;
    return true;
  } else {
    return false;
  }
}

bool isOverChargingCurrent(float current) {
  if (current > 5) {
    isOverCurrentPreviousCharging = true;
  } else {
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


bool isFullyChargeByPercent(int percentToCharge, int batteryPercent) {
  if (batteryPercent >= percentToCharge) {
    isChargingFullyByPercent = true;
    return true;
  } else {
    return false;
  }
}

bool isFullyChargeByAmount(float amountToCharge, float amountChargingSession) {
  if (amountChargingSession >= amountToCharge) {
    isChargingFullyByAmount = true;
    return true;
  } else {
    return false;
  }
}

void OverChargingTemperature() {
  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Over Temperature");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;
}

void OverChargingCurrent() {
  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Over Current");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;
}

void fullyCharge() {
  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Charging Done");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;
}

void fullyCharge2() {
  relayOff();
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Done");
  lcd.setCursor(0, 1);
  lcd.print("Disconnected!");
  delay(2000);

  startTime = 0;
  totalEnergy = 0;
}

void displayChargingOptions() {
  lcd.setCursor(0, 0);
  lcd.print("A:Limit Percent");
  lcd.setCursor(0, 1);
  lcd.print("B:Limit Amount");
  delay(1000);
}

int enterPercentage() {
  // Yêu cầu người dùng nhập phần trăm
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Enter%ToCharge:");
  char key;
  String enteredPercentage = "";

  while (true) {
    key = keypad.getKey();
    if (key) {
      if (key >= '0' && key <= '9') {
        enteredPercentage += key;
        lcd.setCursor(0, 1);
        lcd.print("Charging: ");
        lcd.print(enteredPercentage + "%");
      }
      if (key == '#') {
        chargePercentage = enteredPercentage.toInt();
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Charging to ");
        lcd.print(chargePercentage);
        lcd.print("%");
        delay(2000);  // Hiển thị trong 2 giây
        break;
      }
      if (key == '*') {
        enteredPercentage = "";  // Xóa giá trị đã nhập
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Enter % to charge:");
      }
    }
  }
  return chargePercentage;  // Trả về giá trị phần trăm
}

float enterAmount() {
  // Yêu cầu người dùng nhập số tiền
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Enter $ to charge:");
  char key;
  String enteredAmount = "";

  while (true) {
    key = keypad.getKey();
    if (key) {
      if (key >= '0' && key <= '9') {
        enteredAmount += key;
        lcd.setCursor(0, 1);
        lcd.print("Charging: $");
        lcd.print(enteredAmount);
      }
      if (key == '#') {
        chargeAmount = enteredAmount.toFloat();
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Charging to $");
        lcd.print(chargeAmount);
        delay(2000);  // Hiển thị trong 2 giây
        break;
      }
      if (key == '*') {
        enteredAmount = "";  // Xóa giá trị đã nhập
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("Enter $ to charge:");
      }
    }
  }
  return chargeAmount;  // Trả về giá tiền
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
    } else {
      if (current < 0.3) {
        displayChargingOptions();
        char key = keypad.getKey();
        if (key) {
          if (key == 'A') {
            // Người dùng chọn sạc theo phần trăm
            chargePercentage = enterPercentage();
            bool isFullyChargedByPercent = isFullyChargeByPercent(chargePercentage, battery_percentage);
            if (isFullyChargedByPercent == true || isChargingFullyByPercent == true) {
              fullyCharge2();
            } else {
              charging(voltage, current, temperature, battery_percentage);
            }
          } else if (key == 'B') {
            // Người dùng chọn sạc theo giá tiền
            chargeAmount = enterAmount();
          } else if (key == 'C') {
            charging(voltage, current, temperature, battery_percentage);
          }
        }
      } else {
        charging(voltage, current, temperature, battery_percentage);
      }
    }
  } else {
    if (voltage > 3.2 && temperature < 50) {
      system_ready(battery_percentage, temperature);
    } else {
      system_error(voltage, temperature);
    }
    delay(1000);
  }
}
