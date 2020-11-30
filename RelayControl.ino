#define RELAY 8
void setup() {
  // put your setup code here, to run once:
  pinMode(RELAY, OUTPUT);
  Serial.begin(9600);
}

void loop () {
  if (Serial.available()) {
    int status = Serial.read();
    if (status == '1') {
      digitalWrite(RELAY, HIGH);
    }
    Serial.flush();
  }
  delay(100);
  digitalWrite(RELAY, LOW);
  delay(500);
}
