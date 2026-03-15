# GitHub Actions Deployment Guide

## إعداد Secrets في GitHub

قبل استخدام الـ workflow، يجب إضافة الـ secrets التالية في GitHub:

### الخطوات:
1. اذهب إلى GitHub Repository
2. Settings → Secrets and variables → Actions
3. اضغط "New repository secret"
4. أضف الـ secrets التالية:

### Secrets المطلوبة:

| Secret Name | الوصف | مثال |
|------------|-------|------|
| `SERVER_HOST` | عنوان IP أو اسم النطاق للسيرفر | `192.168.1.100` أو `server.example.com` |
| `SERVER_USER` | اسم المستخدم للاتصال بـ SSH | `abd` |
| `SSH_PRIVATE_KEY` | المفتاح الخاص لـ SSH (المحتوى الكامل) | `-----BEGIN OPENSSH PRIVATE KEY-----...` |
| `SERVER_PORT` | منفذ SSH (اختياري، افتراضي 22) | `22` |
| `SUDO_PASSWORD` | كلمة مرور sudo على السيرفر | `your_password` |

## كيفية الحصول على SSH Private Key

### على السيرفر Linux:
```bash
# إنشاء مفتاح SSH جديد (إذا لم يكن موجود)
ssh-keygen -t ed25519 -C "github-actions"

# عرض المفتاح الخاص
cat ~/.ssh/id_ed25519

# نسخ المفتاح العام إلى authorized_keys
cat ~/.ssh/id_ed25519.pub >> ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
```

### نسخ المفتاح:
1. افتح الملف `~/.ssh/id_ed25519` على السيرفر
2. انسخ المحتوى الكامل (بما في ذلك `-----BEGIN` و `-----END`)
3. الصقه في GitHub Secrets كـ `SSH_PRIVATE_KEY`

## تفعيل الـ Workflow

### التنفيذ التلقائي:
- يتم التنفيذ تلقائياً عند الـ push على فرع `main`

### التنفيذ اليدوي:
1. اذهب إلى Actions في GitHub
2. اختر "Deploy MABA to Linux Server"
3. اضغط "Run workflow"
4. اختر الفرع واضغط "Run workflow"

## ما يقوم به الـ Workflow

### Frontend Deployment:
1. ✅ بناء Angular application
2. ✅ نسخ الملفات إلى `/home/abd/WebSites/MABA/maba-ng`
3. ✅ إنشاء backup تلقائي
4. ✅ إعداد systemd service
5. ✅ تشغيل الخدمة على المنفذ 9087

### Backend Deployment:
1. ✅ بناء .NET API لـ Linux x64
2. ✅ نسخ الملفات إلى `/home/abd/WebSites/MABA/publish`
3. ✅ إنشاء backup تلقائي
4. ✅ إعداد systemd service
5. ✅ تشغيل الخدمة على المنفذ 9090
6. ✅ التحقق من صحة الخدمات

## إدارة الخدمات على السيرفر

### التحقق من حالة الخدمات:
```bash
sudo systemctl status maba-backend.service
sudo systemctl status maba-frontend.service
```

### إعادة تشغيل الخدمات:
```bash
sudo systemctl restart maba-backend.service
sudo systemctl restart maba-frontend.service
```

### عرض الـ logs:
```bash
# Backend logs
sudo journalctl -u maba-backend.service -f

# Frontend logs
sudo journalctl -u maba-frontend.service -f
```

### إيقاف الخدمات:
```bash
sudo systemctl stop maba-backend.service
sudo systemctl stop maba-frontend.service
```

## المتطلبات على السيرفر

### 1. تثبيت .NET 8.0 Runtime:
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 8.0.0 --runtime aspnetcore
```

### 2. تثبيت Node.js و serve:
```bash
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs
sudo npm install -g serve
```

### 3. فتح المنافذ في Firewall:
```bash
sudo ufw allow 9087/tcp  # Frontend
sudo ufw allow 9090/tcp  # Backend
```

## استكشاف الأخطاء

### المشكلة: فشل الاتصال بـ SSH
- ✅ تحقق من `SERVER_HOST` و `SERVER_PORT`
- ✅ تحقق من `SSH_PRIVATE_KEY`
- ✅ تأكد من أن المفتاح موجود في `~/.ssh/authorized_keys`

### المشكلة: فشل تشغيل الخدمة
- ✅ تحقق من الـ logs: `sudo journalctl -u maba-backend.service -n 50`
- ✅ تأكد من وجود `appsettings.json`
- ✅ تحقق من الصلاحيات: `ls -la /home/abd/WebSites/MABA/publish`

### المشكلة: الخدمة لا تبدأ
- ✅ تحقق من .NET Runtime: `dotnet --version`
- ✅ تحقق من المسار: `which dotnet`
- ✅ تحقق من الصلاحيات: `chown -R abd:abd /home/abd/WebSites/MABA`

---

**ملاحظة**: تأكد من تحديث `appsettings.json` على السيرفر بعد النشر الأول!
