# إصلاح مشكلة SQL Server على Linux

## المشكلة
SQL Server driver كان يحاول الوصول إلى Windows Registry على Linux، مما يسبب `NullReferenceException` في `LocalMachineRegistryValue`.

## الحل المطبق

### 1. تحديث Microsoft.Data.SqlClient
تم تحديث `Microsoft.Data.SqlClient` من الإصدار 5.1.6 إلى **5.2.2** في ملف:
- `API/Maba.Infrastructure/Maba.Infrastructure.csproj`

الإصدار 5.2.2 يدعم Linux بشكل أفضل ولا يحاول الوصول إلى Windows Registry.

### 2. تحديث سلسلة الاتصال
تم تحديث سلسلة الاتصال في `publish/appsettings.json`:
```json
"DefaultConnection": "Server=127.0.0.1;Database=MabaDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;Encrypt=False;Integrated Security=false"
```

## الخطوات التالية

### 1. استعادة الحزم
```powershell
cd API
dotnet restore
```

### 2. إعادة بناء المشروع
```powershell
.\build-backend.ps1
```

أو استخدام السكربت الجديد:
```powershell
.\update-sqlclient.ps1
.\build-backend.ps1
```

### 3. نسخ الملفات الجديدة إلى السيرفر
بعد إعادة البناء، انسخ محتويات `publish/backend` إلى السيرفر.

## التحقق من الإصلاح

بعد إعادة البناء والتشغيل، يجب أن:
1. ✅ التطبيق يتصل بقاعدة البيانات بنجاح
2. ✅ لا تظهر أخطاء `NullReferenceException` في الـ logs
3. ✅ الـ migrations تعمل بشكل صحيح

## ملاحظات

- الإصدار 5.2.2 من `Microsoft.Data.SqlClient` يدعم:
  - Windows
  - Linux
  - macOS
  - Docker containers

- تأكد من أن SQL Server يعمل على السيرفر:
  ```bash
  # التحقق من حالة SQL Server
  sudo systemctl status mssql-server
  
  # التحقق من المنفذ
  sudo netstat -tlnp | grep 1433
  ```

---

**تاريخ الإصلاح**: تم تطبيق الإصلاح ✅
