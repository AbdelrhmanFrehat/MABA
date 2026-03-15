# دليل النشر (Deployment Guide)

## الملفات الجاهزة للنشر

تم بناء المشروع بنجاح! الملفات جاهزة في المجلدات التالية:

### الباك إند (Backend)
- **المسار**: `publish\backend`
- **النوع**: .NET 8.0 API
- **الحالة**: ✅ جاهز للنشر

### الفرونت إند (Frontend)
- **المسار**: `publish\frontend`
- **النوع**: Angular Production Build
- **الحالة**: ✅ جاهز للنشر

## سكربتات البناء

تم إنشاء السكربتات التالية لبناء المشروع:

### 1. `build-backend.ps1`
بناء ونشر الباك إند:
```powershell
.\build-backend.ps1
```

### 2. `build-frontend.ps1`
بناء الفرونت إند:
```powershell
.\build-frontend.ps1
```

### 3. `build-all.ps1`
بناء كلا المشروعين معاً:
```powershell
.\build-all.ps1
```

## خطوات النشر على السيرفر

### الباك إند (.NET API)

1. **نسخ الملفات**: انسخ محتويات `publish\backend` إلى السيرفر
2. **متطلبات السيرفر**:
   - .NET 8.0 Runtime (أو .NET 8.0 SDK)
   - SQL Server
   - Windows Server أو Linux (حسب البيئة)

3. **تشغيل التطبيق**:
   ```bash
   dotnet Maba.Api.dll
   ```
   أو استخدام IIS على Windows

4. **إعدادات مهمة**:
   - تأكد من تحديث `appsettings.json` بإعدادات قاعدة البيانات
   - تأكد من إعدادات JWT و SMTP
   - تأكد من إعدادات CORS للسماح بالفرونت إند

### الفرونت إند (Angular)

1. **نسخ الملفات**: انسخ محتويات `publish\frontend` إلى خادم الويب (IIS, Nginx, Apache, etc.)

2. **إعدادات مهمة**:
   - تأكد من أن `environment.prod.ts` يحتوي على رابط API الصحيح
   - تأكد من إعدادات CORS في الباك إند

3. **إعدادات الخادم**:
   - **IIS**: أضف `web.config` مع قاعدة rewrite للـ SPA
   - **Nginx**: أضف قاعدة rewrite للـ Angular routing
   - **Apache**: أضف `.htaccess` مع قاعدة rewrite

### مثال إعداد Nginx

```nginx
server {
    listen 80;
    server_name your-domain.com;
    root /path/to/publish/frontend;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

### مثال إعداد IIS (web.config)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Angular Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

## ملاحظات مهمة

1. **قاعدة البيانات**: تأكد من تطبيق migrations على قاعدة البيانات في السيرفر
2. **الملفات الثابتة**: تأكد من أن مجلد `wwwroot` في الباك إند يحتوي على الملفات المرفوعة
3. **SSL/HTTPS**: يُنصح باستخدام HTTPS في الإنتاج
4. **البيئة**: تأكد من أن `ASPNETCORE_ENVIRONMENT=Production` في السيرفر

## إعادة البناء

لإعادة بناء المشروع بعد التعديلات:

```powershell
# بناء كل شيء
.\build-all.ps1

# أو بناء كل جزء على حدة
.\build-backend.ps1
.\build-frontend.ps1
```

---

**تاريخ البناء الأخير**: تم بناء المشروع بنجاح ✅
