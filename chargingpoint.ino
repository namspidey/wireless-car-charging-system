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

  pinMode(4, OUTPUT);

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
  digitalWrite(4, HIGH);
}

void relayOff() {
  digitalWrite(4, LOW);
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

String receiveChargingData() {
  char receivedText[32] = "";
  if (radio.available()) {
    radio.read(&receivedText, sizeof(receivedText));
    String receivedString = String(receivedText);
    if (receivedString.indexOf("[charging]") != -1) {
      return receivedString;
    }
  }
  return "";
}


void loop() {
  bool detectCar = detectObstacleDigital(SENSOR_SLOT_PIN);
  float point_current = readCurrent(A1);

  if (detectCar) {
    if (point_current >= 0.25) {
      lcd.clear();
      lcd.setCursor(0, 0);
      lcd.print("System Status: ");
      lcd.setCursor(0, 1);
      lcd.print("Charging ! ");

      isCharging = true;
      String dataToSend = receiveChargingData();
      float powerPoint = point_current * 5;

      String powerPointToSend = String("{[charging] Point: ") + String(powerPoint) + "W" + "}";

      Serial.println(dataToSend);
      Serial.println(powerPointToSend);

    } else if (isCharging && point_current < 0.25) {
      system_ready();
      isCharging = false;
      delay(1000);
      Serial.println("[disconnected]");
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
