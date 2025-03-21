#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <SPI.h>
#include <RF24.h>

#define SENSOR_SLOT1_PIN 2

LiquidCrystal_I2C lcd1(0x26, 16, 2);

RF24 radio(9, 10);
const byte address[6] = "00001";

void setup() {
  lcd1.begin(16, 2);
  lcd1.backlight();
  displayMessage(lcd1, "Initializing...", "");
  delay(2000);
  displayMessage(lcd1, "System Check...", "");
  displayMessage(lcd1, "Charging Point 1", "Status: Ready!");

  radio.begin();
  radio.openReadingPipe(1, address);
  radio.setPALevel(RF24_PA_HIGH);
  radio.setChannel(0x4c);
  radio.startListening();

  pinMode(4, OUTPUT);

  Serial.begin(9600);
}

void displayMessage(LiquidCrystal_I2C &lcd, String line1, String line2) {
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print(line1);
  lcd.setCursor(0, 1);
  lcd.print(line2);
}

bool detectObstacleDigital(int sensorDigitalPin) {
  int sensorState = digitalRead(sensorDigitalPin);
  return sensorState == LOW;
}

float readVoltage(int voltage_pin) {
  float totalVoltage = 0.0;
  int sensorValue;

  for (int i = 0; i < 200; i++) {
    sensorValue = analogRead(voltage_pin);
    float voltage = sensorValue * (25.0 / 1023.0) + 0.25;
    totalVoltage += voltage;
    delay(5);
  }

  float averageVoltage = totalVoltage / 200.0;
  return averageVoltage;
}


float readCurrent(int current_pin) {
  float totalCurrent = 0.0;
  int sensorValue;
  float voltage, current;

  for (int i = 0; i < 200; i++) {
    sensorValue = analogRead(current_pin);
    voltage = sensorValue * (5.0 / 1023.0);
    current = (voltage - 2.54) / 0.100;
    totalCurrent += current;
    delay(5);
  }

  float averageCurrent = totalCurrent / 200.0;
  return averageCurrent;
}


String receiveVehicleLicensePlate() {
  char receivedText[32] = "";
  if (radio.available()) {
    radio.read(&receivedText, sizeof(receivedText));
    String licensePlate = String(receivedText);
    return licensePlate;
  } else {
    Serial.print("Not found");
    return "";
  }
}

void relayOn() {
  digitalWrite(4, HIGH);
}

void relayOff() {
  digitalWrite(4, LOW);
}

void loop() {
  Serial.println(receiveVehicleLicensePlate());
  delay(1000);
}
