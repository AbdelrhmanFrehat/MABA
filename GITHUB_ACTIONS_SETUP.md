# إعداد GitHub Actions للنشر التلقائي

## ✅ ما تم إنجازه

تم إنشاء ملف GitHub Actions workflow جاهز للنشر التلقائي على السيرفر Linux.

## 📁 الملفات المنشأة

1. **`.github/workflows/deploy.yml`** - ملف الـ workflow الرئيسي
2. **`.github/workflows/README.md`** - دليل شامل للاستخدام
3. **`.gitignore`** - تحديث لاستثناء ملفات البناء

## 🚀 الخطوات السريعة

### 1. إعداد Secrets في GitHub

اذهب إلى: `Settings → Secrets and variables → Actions`

أضف الـ secrets التالية:

| Secret | الوصف |
|--------|-------|
| `SERVER_HOST` | عنوان IP السيرفر (مثال: `192.168.1.100`) |
| `SERVER_USER` | اسم المستخدم SSH (مثال: `abd`) |
| `SSH_PRIVATE_KEY` | محتوى المفتاح الخاص SSH |
| `SERVER_PORT` | منفذ SSH (افتراضي: `22`) |
| `SUDO_PASSWORD` | كلمة مرور sudo على السيرفر |

### 2. الحصول على SSH Private Key

على السيرفر Linux:
```bash
# إنشاء مفتاح جديد (إذا لم يكن موجود)
ssh-keygen -t ed25519 -C "github-actions"

# عرض المفتاح الخاص
cat ~/.ssh/id_ed25519

# نسخ المفتاح العام
cat ~/.ssh/id_ed25519.pub >> ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
```

انسخ محتوى `~/.ssh/id_ed25519` كاملاً (بما في ذلك `-----BEGIN` و `-----END`) والصقه في GitHub Secrets.

### 3. رفع الملفات إلى GitHub

```bash
git add .github/workflows/deploy.yml
git add .github/workflows/README.md
git commit -m "Add GitHub Actions deployment workflow"
git push origin main
```

### 4. تفعيل الـ Workflow

#### التنفيذ التلقائي:
- يتم تلقائياً عند الـ push على فرع `main`

#### التنفيذ اليدوي:
1. اذهب إلى **Actions** في GitHub
2. اختر **"Deploy MABA to Linux Server"**
3. اضغط **"Run workflow"**
4. اختر الفرع واضغط **"Run workflow"**

## 📋 ما يقوم به الـ Workflow

### Frontend:
1. ✅ بناء Angular application
2. ✅ نسخ إلى `/home/abd/WebSites/MABA/maba-ng`
3. ✅ إنشاء backup تلقائي
4. ✅ إعداد systemd service
5. ✅ تشغيل على المنفذ **9087**

### Backend:
1. ✅ بناء .NET API لـ **Linux x64**
2. ✅ نسخ إلى `/home/abd/WebSites/MABA/publish`
3. ✅ إنشاء backup تلقائي
4. ✅ إعداد systemd service
5. ✅ تشغيل على المنفذ **9090**
6. ✅ Health check تلقائي

## 🔧 متطلبات السيرفر

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

### 3. فتح المنافذ:
```bash
sudo ufw allow 9087/tcp  # Frontend
sudo ufw allow 9090/tcp  # Backend
```

## 📝 ملاحظات مهمة

1. **appsettings.json**: تأكد من وجوده على السيرفر بعد النشر الأول
2. **الصلاحيات**: تأكد من أن المستخدم `abd` لديه صلاحيات على المجلدات
3. **Backups**: يتم إنشاء backups تلقائياً في `/home/abd/WebSites/MABA/backups`

## 🔍 التحقق من النشر

بعد النشر، تحقق من:
```bash
# حالة الخدمات
sudo systemctl status maba-backend.service
sudo systemctl status maba-frontend.service

# الـ logs
sudo journalctl -u maba-backend.service -f
sudo journalctl -u maba-frontend.service -f

# اختبار الـ endpoints
curl http://localhost:9090/swagger  # Backend
curl http://localhost:9087           # Frontend
```

## 🆘 استكشاف الأخطاء

### فشل الاتصال بـ SSH:
- ✅ تحقق من `SERVER_HOST` و `SERVER_PORT`
- ✅ تحقق من `SSH_PRIVATE_KEY` (يجب أن يكون كاملاً)
- ✅ تأكد من وجود المفتاح في `~/.ssh/authorized_keys`

### فشل تشغيل الخدمة:
- ✅ تحقق من الـ logs: `sudo journalctl -u maba-backend.service -n 50`
- ✅ تأكد من وجود `.NET Runtime`: `dotnet --version`
- ✅ تحقق من الصلاحيات: `ls -la /home/abd/WebSites/MABA/publish`

---

**جاهز للاستخدام! 🎉**

بعد إضافة الـ secrets، ارفع الكود إلى GitHub وسيعمل النشر تلقائياً!
