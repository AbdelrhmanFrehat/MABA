# بناء المشروع لـ Linux x64

## السكربتات المتاحة

### 1. `build-backend-linux.ps1` (PowerShell)
```powershell
.\build-backend-linux.ps1
```

### 2. `build-backend-linux.bat` (Batch)
```cmd
build-backend-linux.bat
```

## البناء اليدوي

إذا كنت تفضل البناء اليدوي:

```bash
# استعادة الحزم
cd API
dotnet restore Maba.Api/Maba.Api.csproj --runtime linux-x64

# البناء
dotnet build Maba.Api/Maba.Api.csproj --configuration Release

# النشر
dotnet publish Maba.Api/Maba.Api.csproj `
    --configuration Release `
    --output ../publish/backend-linux `
    --self-contained false `
    --runtime linux-x64
```

## الملفات الناتجة

بعد البناء، ستجد الملفات في:
- `publish/backend-linux/`

## متطلبات السيرفر Linux

### 1. تثبيت .NET 8.0 Runtime

**Ubuntu/Debian:**
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 8.0.0 --runtime aspnetcore
```

**أو استخدام apt:**
```bash
sudo apt-get update
sudo apt-get install -y dotnet-runtime-8.0 aspnetcore-runtime-8.0
```

### 2. نسخ الملفات إلى السيرفر

```bash
# نسخ الملفات
scp -r publish/backend-linux/* user@server:/path/to/app/

# أو استخدام rsync
rsync -avz publish/backend-linux/ user@server:/path/to/app/
```

### 3. تشغيل التطبيق على Linux

```bash
cd /path/to/app
dotnet Maba.Api.dll
```

### 4. تشغيل كخدمة (systemd)

إنشاء ملف خدمة `/etc/systemd/system/maba-api.service`:

```ini
[Unit]
Description=MABA API Service
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/maba-api
ExecStart=/usr/bin/dotnet /var/www/maba-api/Maba.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=maba-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

تفعيل وتشغيل الخدمة:
```bash
sudo systemctl daemon-reload
sudo systemctl enable maba-api
sudo systemctl start maba-api
sudo systemctl status maba-api
```

## ملاحظات مهمة

1. **Microsoft.Data.SqlClient 5.2.2**: تم تحديثه لدعم Linux بشكل أفضل
2. **سلسلة الاتصال**: تأكد من تحديث `appsettings.json` على السيرفر
3. **الصلاحيات**: تأكد من أن المستخدم لديه صلاحيات القراءة والكتابة
4. **المنافذ**: تأكد من فتح المنفذ المطلوب (عادة 5000 أو 80)

## التحقق من البناء

بعد البناء، تحقق من:
- وجود `Maba.Api.dll` في المجلد
- وجود جميع ملفات `.dll` المطلوبة
- وجود `appsettings.json` و `appsettings.Development.json`
- وجود مجلد `wwwroot` للملفات الثابتة

---

**تاريخ الإنشاء**: تم إنشاء السكربتات ✅
