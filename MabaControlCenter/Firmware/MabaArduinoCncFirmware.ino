const int X_STEP = 2;
const int Y_STEP = 3;
const int Z_STEP = 4;

const int X_DIR  = 5;
const int Y_DIR  = 6;
const int Z_DIR  = 7;

const int EN_PIN = 8;

const int X_LIMIT = 9;
const int Y_LIMIT = 10;

int speedDelay = 200;
String cmd = "";

bool X_HOME_DIR = LOW;
bool Y_HOME_DIR = LOW;

void pulseStep(int stepPin) {
  digitalWrite(stepPin, HIGH);
  delayMicroseconds(speedDelay);
  digitalWrite(stepPin, LOW);
  delayMicroseconds(speedDelay);
}

void stepAxis(int stepPin, int dirPin, long steps, bool dir) {
  if (steps <= 0) return;

  digitalWrite(dirPin, dir);
  delay(2);

  for (long i = 0; i < steps; i++) {
    pulseStep(stepPin);
  }
}

void moveXY(long stepsX, bool dirX, long stepsY, bool dirY) {
  if (stepsX <= 0 && stepsY <= 0) return;

  digitalWrite(X_DIR, dirX);
  digitalWrite(Y_DIR, dirY);
  delay(2);

  long maxSteps = max(stepsX, stepsY);

  for (long i = 0; i < maxSteps; i++) {
    if (i < stepsX) digitalWrite(X_STEP, HIGH);
    if (i < stepsY) digitalWrite(Y_STEP, HIGH);

    delayMicroseconds(speedDelay);

    if (i < stepsX) digitalWrite(X_STEP, LOW);
    if (i < stepsY) digitalWrite(Y_STEP, LOW);

    delayMicroseconds(speedDelay);
  }
}

void moveXYSigned(long signedX, long signedY) {
  long stepsX = labs(signedX);
  long stepsY = labs(signedY);

  bool dirX = signedX >= 0;
  bool dirY = signedY >= 0;

  moveXY(stepsX, dirX, stepsY, dirY);
}

void squareMove(long size) {
  moveXY(size, HIGH, 0, HIGH);
  delay(200);
  moveXY(0, HIGH, size, HIGH);
  delay(200);
  moveXY(size, LOW, 0, HIGH);
  delay(200);
  moveXY(0, HIGH, size, LOW);
  delay(200);
}

void homeAxisNC(int stepPin, int dirPin, int limitPin, bool homeDir) {
  digitalWrite(dirPin, homeDir);
  delay(2);

  while (digitalRead(limitPin) == LOW) {
    pulseStep(stepPin);
  }

  delay(200);

  digitalWrite(dirPin, !homeDir);
  for (int i = 0; i < 100; i++) {
    pulseStep(stepPin);
  }

  delay(200);

  digitalWrite(dirPin, homeDir);
  while (digitalRead(limitPin) == LOW) {
    pulseStep(stepPin);
  }
}

void homeXY() {
  homeAxisNC(X_STEP, X_DIR, X_LIMIT, X_HOME_DIR);
  homeAxisNC(Y_STEP, Y_DIR, Y_LIMIT, Y_HOME_DIR);
  Serial.println("HOME DONE");
}

bool parseSignedPair(String payload, long &xValue, long &yValue) {
  int firstComma = payload.indexOf(',');
  int secondComma = payload.indexOf(',', firstComma + 1);

  if (firstComma < 0 || secondComma < 0) return false;

  String xText = payload.substring(firstComma + 1, secondComma);
  String yText = payload.substring(secondComma + 1);

  xText.trim();
  yText.trim();

  if (xText.length() == 0 || yText.length() == 0) return false;

  xValue = xText.toInt();
  yValue = yText.toInt();
  return true;
}

void parseAndMove(String s) {
  s.trim();
  if (s.length() == 0) return;

  if (s.equalsIgnoreCase("B")) {
    squareMove(400);
    return;
  }

  if (s.equalsIgnoreCase("H")) {
    homeXY();
    return;
  }

  if (s.startsWith("XY,") || s.startsWith("xy,")) {
    long signedX = 0;
    long signedY = 0;

    if (parseSignedPair(s, signedX, signedY)) {
      moveXYSigned(signedX, signedY);
    }

    return;
  }

  char sign = s.charAt(0);
  char axis = s.charAt(s.length() - 1);
  String numStr = s.substring(1, s.length() - 1);
  long steps = numStr.toInt();

  if ((sign != '+') && (sign != '-')) return;
  if (steps <= 0) return;

  bool dir = (sign == '+');

  if (axis == 'x' || axis == 'X') {
    stepAxis(X_STEP, X_DIR, steps, dir);
  } else if (axis == 'y' || axis == 'Y') {
    stepAxis(Y_STEP, Y_DIR, steps, dir);
  } else if (axis == 'z' || axis == 'Z') {
    stepAxis(Z_STEP, Z_DIR, steps, dir);
  }
}

void setup() {
  pinMode(X_STEP, OUTPUT);
  pinMode(Y_STEP, OUTPUT);
  pinMode(Z_STEP, OUTPUT);

  pinMode(X_DIR, OUTPUT);
  pinMode(Y_DIR, OUTPUT);
  pinMode(Z_DIR, OUTPUT);

  pinMode(EN_PIN, OUTPUT);
  digitalWrite(EN_PIN, LOW);

  pinMode(X_LIMIT, INPUT_PULLUP);
  pinMode(Y_LIMIT, INPUT_PULLUP);

  Serial.begin(115200);
  Serial.println("Ready: +100x  -100y  +100z  XY,120,-120  B  H");
}

void loop() {
  while (Serial.available()) {
    char c = Serial.read();

    if (c == '\n' || c == '\r') {
      if (cmd.length() > 0) {
        parseAndMove(cmd);
        cmd = "";
      }
    } else {
      cmd += c;
    }
  }
}
