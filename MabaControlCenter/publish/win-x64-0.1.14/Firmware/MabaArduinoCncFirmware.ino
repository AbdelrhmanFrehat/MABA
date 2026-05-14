// MABA_FIRMWARE_VERSION: 2.0.0
// TARGET_BOARD: Arduino Uno (arduino:avr:uno)

const int X_STEP = 2;
const int Y_STEP = 3;
const int Z_STEP = 4;

const int X_DIR = 5;
const int Y_DIR = 6;
const int Z_DIR = 7;

const int EN_PIN = 8;

const int X_LIMIT = 9;
const int Y_LIMIT = 10;

const int SPINDLE_PIN = 11;

float stepsPerMM_X = 80.0;
float stepsPerMM_Y = 80.0;
float stepsPerMM_Z = 400.0;

float posX = 0;
float posY = 0;
float posZ = 0;

float maxX = 400;
float maxY = 330;
float maxZ = 50;

int stepDelay = 200;

bool X_HOME_DIR = LOW;
bool Y_HOME_DIR = LOW;
bool Z_HOME_DIR = LOW;

bool unlocked = false;
bool alarm = false;

String cmd = "";

void pulseStep(int pin) {
  digitalWrite(pin, HIGH);
  delayMicroseconds(stepDelay);
  digitalWrite(pin, LOW);
  delayMicroseconds(stepDelay);
}

void setDir(int dirPin, bool dir) {
  digitalWrite(dirPin, dir);
  delayMicroseconds(10);
}

bool limitsHit() {
  if (digitalRead(X_LIMIT) == HIGH) return true;
  if (digitalRead(Y_LIMIT) == HIGH) return true;
  return false;
}

void emergencyStop() {
  digitalWrite(EN_PIN, HIGH);
  digitalWrite(SPINDLE_PIN, LOW);
  alarm = true;
  unlocked = false;
  Serial.println("ALARM:LIMIT_OR_ESTOP");
}

void stepAxisRaw(int stepPin, int dirPin, long steps, bool dir) {
  setDir(dirPin, dir);

  for (long i = 0; i < steps; i++) {
    if (limitsHit()) {
      emergencyStop();
      return;
    }

    pulseStep(stepPin);
  }
}

void moveLinearMM(float targetX, float targetY, float targetZ, float feed) {
  if (!unlocked || alarm) {
    Serial.println("ERR:LOCKED");
    return;
  }

  if (targetX < 0 || targetX > maxX || targetY < 0 || targetY > maxY || targetZ < 0 || targetZ > maxZ) {
    Serial.println("ERR:SOFT_LIMIT");
    return;
  }

  long dx = round((targetX - posX) * stepsPerMM_X);
  long dy = round((targetY - posY) * stepsPerMM_Y);
  long dz = round((targetZ - posZ) * stepsPerMM_Z);

  bool dirX = dx >= 0;
  bool dirY = dy >= 0;
  bool dirZ = dz >= 0;

  dx = abs(dx);
  dy = abs(dy);
  dz = abs(dz);

  setDir(X_DIR, dirX);
  setDir(Y_DIR, dirY);
  setDir(Z_DIR, dirZ);

  long maxSteps = max(dx, max(dy, dz));

  long accX = 0;
  long accY = 0;
  long accZ = 0;

  for (long i = 0; i < maxSteps; i++) {
    if (limitsHit()) {
      emergencyStop();
      return;
    }

    accX += dx;
    accY += dy;
    accZ += dz;

    bool sx = false;
    bool sy = false;
    bool sz = false;

    if (accX >= maxSteps) {
      accX -= maxSteps;
      sx = true;
    }

    if (accY >= maxSteps) {
      accY -= maxSteps;
      sy = true;
    }

    if (accZ >= maxSteps) {
      accZ -= maxSteps;
      sz = true;
    }

    if (sx) digitalWrite(X_STEP, HIGH);
    if (sy) digitalWrite(Y_STEP, HIGH);
    if (sz) digitalWrite(Z_STEP, HIGH);

    delayMicroseconds(stepDelay);

    if (sx) digitalWrite(X_STEP, LOW);
    if (sy) digitalWrite(Y_STEP, LOW);
    if (sz) digitalWrite(Z_STEP, LOW);

    delayMicroseconds(stepDelay);
  }

  posX = targetX;
  posY = targetY;
  posZ = targetZ;

  Serial.println("OK");
}

void homeAxisNC(int stepPin, int dirPin, int limitPin, bool homeDir) {
  setDir(dirPin, homeDir);

  while (digitalRead(limitPin) == LOW) {
    pulseStep(stepPin);
  }

  delay(200);

  setDir(dirPin, !homeDir);

  for (int i = 0; i < 200; i++) {
    pulseStep(stepPin);
  }

  delay(200);

  setDir(dirPin, homeDir);

  while (digitalRead(limitPin) == LOW) {
    pulseStep(stepPin);
  }

  delay(200);

  setDir(dirPin, !homeDir);

  for (int i = 0; i < 100; i++) {
    pulseStep(stepPin);
  }
}

void homeMachine() {
  digitalWrite(EN_PIN, LOW);
  alarm = false;

  homeAxisNC(X_STEP, X_DIR, X_LIMIT, X_HOME_DIR);
  homeAxisNC(Y_STEP, Y_DIR, Y_LIMIT, Y_HOME_DIR);

  posX = 0;
  posY = 0;
  posZ = 0;

  unlocked = true;

  Serial.println("HOME:DONE");
}

float getValue(String s, char key, float oldValue) {
  int index = s.indexOf(key);

  if (index == -1) return oldValue;

  int endIndex = index + 1;

  while (endIndex < s.length()) {
    char c = s.charAt(endIndex);

    if ((c >= '0' && c <= '9') || c == '.' || c == '-') {
      endIndex++;
    } else {
      break;
    }
  }

  return s.substring(index + 1, endIndex).toFloat();
}

void moveArc(bool clockwise, float endX, float endY, float i, float j, float feed) {
  float centerX = posX + i;
  float centerY = posY + j;

  float startAngle = atan2(posY - centerY, posX - centerX);
  float endAngle = atan2(endY - centerY, endX - centerX);

  float radius = sqrt(i * i + j * j);

  float angleTravel = endAngle - startAngle;

  if (clockwise && angleTravel >= 0) angleTravel -= 2 * PI;
  if (!clockwise && angleTravel <= 0) angleTravel += 2 * PI;

  int segments = abs(angleTravel) * radius * 4;

  if (segments < 60) segments = 60;
  if (segments > 720) segments = 720;

  for (int n = 1; n <= segments; n++) {
    float angle = startAngle + angleTravel * ((float)n / segments);

    float x = centerX + cos(angle) * radius;
    float y = centerY + sin(angle) * radius;

    moveLinearMM(x, y, posZ, feed);

    if (alarm) return;
  }
}

void reportStatus() {
  Serial.print("<MABA:");
  Serial.print(unlocked ? "READY" : "LOCKED");
  Serial.print("|ALARM:");
  Serial.print(alarm ? "1" : "0");
  Serial.print("|X:");
  Serial.print(posX, 3);
  Serial.print("|Y:");
  Serial.print(posY, 3);
  Serial.print("|Z:");
  Serial.print(posZ, 3);
  Serial.print("|XLIM:");
  Serial.print(digitalRead(X_LIMIT));
  Serial.print("|YLIM:");
  Serial.print(digitalRead(Y_LIMIT));
  Serial.println(">");
}

void parseCommand(String s) {
  s.trim();
  s.toUpperCase();

  if (s.length() == 0) return;

  if (s == "?") {
    reportStatus();
    return;
  }

  if (s == "$H" || s == "H") {
    homeMachine();
    return;
  }

  if (s == "$X") {
    alarm = false;
    unlocked = true;
    digitalWrite(EN_PIN, LOW);
    Serial.println("UNLOCKED");
    return;
  }

  if (s == "!") {
    emergencyStop();
    return;
  }

  if (s == "M3") {
    digitalWrite(SPINDLE_PIN, HIGH);
    Serial.println("SPINDLE:ON");
    return;
  }

  if (s == "M5") {
    digitalWrite(SPINDLE_PIN, LOW);
    Serial.println("SPINDLE:OFF");
    return;
  }

  if (s.startsWith("G0") || s.startsWith("G1")) {
    float x = getValue(s, 'X', posX);
    float y = getValue(s, 'Y', posY);
    float z = getValue(s, 'Z', posZ);
    float f = getValue(s, 'F', 300);

    moveLinearMM(x, y, z, f);
    return;
  }

  if (s.startsWith("G2") || s.startsWith("G3")) {
    bool clockwise = s.startsWith("G2");

    float x = getValue(s, 'X', posX);
    float y = getValue(s, 'Y', posY);
    float i = getValue(s, 'I', 0);
    float j = getValue(s, 'J', 0);
    float f = getValue(s, 'F', 300);

    moveArc(clockwise, x, y, i, j, f);
    Serial.println("OK");
    return;
  }

  if (s.startsWith("JX")) {
    float distance = getValue(s, 'X', 0);
    moveLinearMM(posX + distance, posY, posZ, 300);
    return;
  }

  if (s.startsWith("JY")) {
    float distance = getValue(s, 'Y', 0);
    moveLinearMM(posX, posY + distance, posZ, 300);
    return;
  }

  if (s.startsWith("JZ")) {
    float distance = getValue(s, 'Z', 0);
    moveLinearMM(posX, posY, posZ + distance, 300);
    return;
  }

  Serial.println("ERR:UNKNOWN_COMMAND");
}

void setup() {
  pinMode(X_STEP, OUTPUT);
  pinMode(Y_STEP, OUTPUT);
  pinMode(Z_STEP, OUTPUT);

  pinMode(X_DIR, OUTPUT);
  pinMode(Y_DIR, OUTPUT);
  pinMode(Z_DIR, OUTPUT);

  pinMode(EN_PIN, OUTPUT);
  pinMode(SPINDLE_PIN, OUTPUT);

  pinMode(X_LIMIT, INPUT_PULLUP);
  pinMode(Y_LIMIT, INPUT_PULLUP);

  digitalWrite(EN_PIN, HIGH);
  digitalWrite(SPINDLE_PIN, LOW);

  Serial.begin(115200);

  Serial.println("MABA CNC FIRMWARE READY");
  Serial.println("LOCKED: SEND $H TO HOME OR $X TO UNLOCK");
}

void loop() {
  while (Serial.available()) {
    char c = Serial.read();

    if (c == '\n' || c == '\r') {
      if (cmd.length() > 0) {
        parseCommand(cmd);
        cmd = "";
      }
    } else {
      cmd += c;
    }
  }
}
