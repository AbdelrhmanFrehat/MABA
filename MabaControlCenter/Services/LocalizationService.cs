using System.IO;
using System.Windows;

namespace MabaControlCenter.Services;

public class LocalizationService : ILocalizationService
{
    private static string CultureFilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MabaControlCenter", "culture.txt");

    private readonly Dictionary<string, string> _en = new();
    private readonly Dictionary<string, string> _ar = new();
    private string _currentCulture = "en";

    public string CurrentCulture => _currentCulture;
    public FlowDirection FlowDirection => _currentCulture == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public event EventHandler? CultureChanged;

    public LocalizationService()
    {
        LoadEnglish();
        LoadArabic();
        LoadSavedCulture();
    }

    private void LoadSavedCulture()
    {
        try
        {
            if (File.Exists(CultureFilePath))
            {
                var saved = File.ReadAllText(CultureFilePath).Trim().ToLowerInvariant();
                if (saved == "ar" || saved == "en")
                    _currentCulture = saved;
            }
        }
        catch { /* ignore */ }
    }

    private static void SaveCulture(string cultureCode)
    {
        try
        {
            var dir = Path.GetDirectoryName(CultureFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(CultureFilePath, cultureCode);
        }
        catch { /* ignore */ }
    }

    public string GetString(string key)
    {
        var dict = _currentCulture == "ar" ? _ar : _en;
        return dict.TryGetValue(key, out var value) ? value : _en.TryGetValue(key, out value) ? value : key;
    }

    public void SetCulture(string cultureCode)
    {
        var next = cultureCode?.ToLowerInvariant() == "ar" ? "ar" : "en";
        if (_currentCulture == next) return;
        _currentCulture = next;
        SaveCulture(_currentCulture);
        CultureChanged?.Invoke(this, EventArgs.Empty);

        if (Application.Current?.MainWindow != null)
            Application.Current.MainWindow.FlowDirection = FlowDirection;
    }

    private void LoadEnglish()
    {
        _en["Sidebar_Dashboard"] = "Dashboard";
        _en["Sidebar_Devices"] = "Devices";
        _en["Sidebar_Commands"] = "Commands";
        _en["Sidebar_Modules"] = "Modules";
        _en["Sidebar_Logs"] = "Logs";
        _en["Sidebar_Settings"] = "Settings";
        _en["App_Title"] = "Maba Control Center";
        _en["Page_Dashboard_Title"] = "Dashboard";
        _en["Page_Dashboard_Placeholder"] = "Overview and status.";
        _en["Page_Devices_Title"] = "Devices";
        _en["Page_Devices_Placeholder"] = "Devices — connect to a serial port below.";
        _en["Page_Commands_Title"] = "Commands";
        _en["Page_Commands_Placeholder"] = "Send text commands (e.g. PING, STATUS, START, STOP).";
        _en["Page_Modules_Title"] = "Modules";
        _en["Page_Modules_Placeholder"] = "Available modules for MABA devices.";
        _en["Page_Logs_Title"] = "Logs";
        _en["Page_Logs_Placeholder"] = "Central log — all sent and received messages.";
        _en["Page_Settings_Title"] = "Settings";
        _en["Page_Settings_Placeholder"] = "Application configuration.";
        _en["Page_DexterCalibration_Title"] = "Dexter MacroPad Configurator";
        _en["Page_DexterCalibration_Placeholder"] = "This is a placeholder for the module UI.";
        _en["Button_BackToModules"] = "← Back to Modules";
        _en["Label_ComPort"] = "COM port:";
        _en["Label_Status"] = "Status:";
        _en["Label_Theme"] = "Theme";
        _en["Label_Output"] = "Output";
        _en["Label_DeviceInfo"] = "Device information";
        _en["Label_NoDeviceDetected"] = "No supported device detected.";
        _en["Label_ProductName"] = "Product Name:";
        _en["Label_ProductCode"] = "Product Code:";
        _en["Label_FirmwareVersion"] = "Firmware Version:";
        _en["Label_SerialNumber"] = "Serial Number:";
        _en["Label_ConnectionType"] = "Connection Type:";
        _en["Label_RecommendedModule"] = "Recommended Module:";
        _en["Label_SupportStatus"] = "Support Status:";
        _en["Label_Version"] = "Version: ";
        _en["Label_Installed"] = "Installed:";
        _en["Label_Enabled"] = "Enabled:";
        _en["Label_CurrentVersion"] = "Current version:";
        _en["Label_LatestVersion"] = "Latest version:";
        _en["Label_ConnectedDevice"] = "Connected device";
        _en["Label_RecommendedModuleSection"] = "Recommended module";
        _en["Label_AvailableModules"] = "Available modules";
        _en["Label_AppVersionUpdates"] = "App version & updates";
        _en["Label_WhatsNew"] = "What's New";
        _en["Button_Refresh"] = "Refresh";
        _en["Button_Connect"] = "Connect";
        _en["Button_Disconnect"] = "Disconnect";
        _en["Button_Send"] = "Send";
        _en["Button_CheckForUpdates"] = "Check for Updates";
        _en["Button_ClearLogs"] = "Clear Logs";
        _en["Button_ExportToTxt"] = "Export to .txt";
        _en["Status_Connected"] = "Connected";
        _en["Status_Disconnected"] = "Disconnected";
        _en["Label_RecommendedForDevice"] = "Recommended for connected device";
        _en["Label_Yes"] = "Yes";
        _en["Label_No"] = "No";
        _en["Format_ModulesCount"] = "{0} modules";
        _en["Language_English"] = "English";
        _en["Language_Arabic"] = "Arabic";
        _en["Label_Language"] = "Language";
        _en["Logs_Timestamp"] = "Timestamp";
        _en["Logs_Direction"] = "Direction";
        _en["Logs_Message"] = "Message";
        _en["Logs_Status"] = "Status";
        _en["Device_NoDevice_Title"] = "No device detected";
        _en["Device_NoDevice_Message"] = "Make sure your device is connected and the correct port is selected.";
        _en["Device_NoDevice_Suggestion_USB"] = "Check USB connection";
        _en["Device_NoDevice_Suggestion_Port"] = "Select the correct COM port";
        _en["Device_NoDevice_Suggestion_Refresh"] = "Click Refresh";
        _en["Device_NoDevice_Suggestion_Drivers"] = "Install required drivers";
        _en["Device_NoDevice_Button_Drivers"] = "Download Drivers";
        _en["Device_NoDevice_Button_Support"] = "Open Support Page";
        _en["Device_NoDevice_Button_Troubleshooting"] = "Troubleshooting Guide";
        _en["Device_Unsupported_Title"] = "Unsupported device detected";
        _en["Device_Unsupported_Button_RequestSupport"] = "Request Support";
        _en["Device_Unsupported_Button_Website"] = "Open Website";
        _en["Settings_Appearance"] = "Appearance";
        _en["Settings_AppearanceHelp"] = "Choose how the app looks.";
        _en["Settings_LanguageHelp"] = "English is default. Arabic supports RTL.";
        _en["Settings_AppPreferences"] = "App Preferences";
        _en["Settings_StartWithWindows"] = "Start with Windows";
        _en["Settings_CheckForUpdatesAuto"] = "Check for updates automatically";
        _en["Settings_DiagnosticsMode"] = "Diagnostics mode";
        _en["Settings_About"] = "About";
        _en["Settings_Label_AppName"] = "App name:";
        _en["Settings_AppName"] = "Maba Control Center";
        _en["Settings_Version"] = "0.1.0";
        _en["Settings_Label_Company"] = "Company:";
        _en["Settings_Company"] = "MABA Solutions";
        _en["Sidebar_Discover"] = "Discover";
        _en["Page_Discover_Title"] = "Discover";
        _en["Page_Discover_Placeholder"] = "Products, modules, and quick links.";
        _en["Discover_FeaturedProducts"] = "Featured Products";
        _en["Discover_FeaturedModules"] = "Featured Modules";
        _en["Discover_QuickActions"] = "Quick Actions";
        _en["Discover_Product_DexterVP1"] = "MABA Dexter VP1";
        _en["Discover_Product_Scara"] = "MABA SCARA";
        _en["Discover_Product_CNC"] = "MABA CNC Platform";
        _en["Discover_Module_DexterCal"] = "Dexter MacroPad";
        _en["Discover_Module_ScaraControl"] = "SCARA Control";
        _en["Discover_Module_CNCManager"] = "CNC Manager";
        _en["Discover_Action_Website"] = "Open MABA Website";
        _en["Discover_Action_StartProject"] = "Start a Project";
        _en["Discover_Action_Support"] = "Support & Downloads";
        _en["Discover_Product_DexterVP1_Desc"] = "Precision control platform for MABA Dexter systems.";
        _en["Discover_Product_Scara_Desc"] = "Industrial robotic arm platform for motion and automation.";
        _en["Discover_Product_CNC_Desc"] = "Integrated CNC control and monitoring environment.";
        _en["Discover_Module_DexterCal_Desc"] = "MacroPad key mapping (a–p) for Dexter devices.";
        _en["Discover_Module_ScaraControl_Desc"] = "Robot control tools for SCARA systems.";
        _en["Discover_Module_CNCManager_Desc"] = "Monitoring and control tools for CNC devices.";
        _en["Discover_Status_Available"] = "Available";
        _en["Discover_Status_ComingSoon"] = "Coming Soon";
        _en["Cnc_Tab_Operate"] = "Operate";
        _en["Cnc_Tab_Monitor"] = "Monitor";
        _en["Cnc_Tab_Setup"] = "Setup";
    }

    private void LoadArabic()
    {
        _ar["Sidebar_Dashboard"] = "لوحة التحكم";
        _ar["Sidebar_Devices"] = "الأجهزة";
        _ar["Sidebar_Commands"] = "الأوامر";
        _ar["Sidebar_Modules"] = "الوحدات";
        _ar["Sidebar_Logs"] = "السجلات";
        _ar["Sidebar_Settings"] = "الإعدادات";
        _ar["App_Title"] = "مركز تحكم MABA";
        _ar["Page_Dashboard_Title"] = "لوحة التحكم";
        _ar["Page_Dashboard_Placeholder"] = "نظرة عامة والحالة.";
        _ar["Page_Devices_Title"] = "الأجهزة";
        _ar["Page_Devices_Placeholder"] = "الأجهزة — الاتصال بمنفذ تسلسلي أدناه.";
        _ar["Page_Commands_Title"] = "الأوامر";
        _ar["Page_Commands_Placeholder"] = "إرسال أوامر نصية (مثل PING، STATUS، START، STOP).";
        _ar["Page_Modules_Title"] = "الوحدات";
        _ar["Page_Modules_Placeholder"] = "الوحدات المتاحة لأجهزة MABA.";
        _ar["Page_Logs_Title"] = "السجلات";
        _ar["Page_Logs_Placeholder"] = "السجل المركزي — جميع الرسائل المرسلة والمستلمة.";
        _ar["Page_Settings_Title"] = "الإعدادات";
        _ar["Page_Settings_Placeholder"] = "إعدادات التطبيق.";
        _ar["Page_DexterCalibration_Title"] = "منسق ماكرو باد دكستر";
        _ar["Page_DexterCalibration_Placeholder"] = "هذا عنصر نائب لواجهة الوحدة.";
        _ar["Button_BackToModules"] = "→ العودة إلى الوحدات";
        _ar["Label_ComPort"] = "منفذ COM:";
        _ar["Label_Status"] = "الحالة:";
        _ar["Label_Theme"] = "المظهر";
        _ar["Label_Output"] = "المخرجات";
        _ar["Label_DeviceInfo"] = "معلومات الجهاز";
        _ar["Label_NoDeviceDetected"] = "لم يتم اكتشاف جهاز مدعوم.";
        _ar["Label_ProductName"] = "اسم المنتج:";
        _ar["Label_ProductCode"] = "رمز المنتج:";
        _ar["Label_FirmwareVersion"] = "إصدار البرنامج الثابت:";
        _ar["Label_SerialNumber"] = "الرقم التسلسلي:";
        _ar["Label_ConnectionType"] = "نوع الاتصال:";
        _ar["Label_RecommendedModule"] = "الوحدة الموصى بها:";
        _ar["Label_SupportStatus"] = "حالة الدعم:";
        _ar["Label_Version"] = "الإصدار: ";
        _ar["Label_Installed"] = "مثبت:";
        _ar["Label_Enabled"] = "مفعّل:";
        _ar["Label_CurrentVersion"] = "الإصدار الحالي:";
        _ar["Label_LatestVersion"] = "آخر إصدار:";
        _ar["Label_ConnectedDevice"] = "الجهاز المتصل";
        _ar["Label_RecommendedModuleSection"] = "الوحدة الموصى بها";
        _ar["Label_AvailableModules"] = "الوحدات المتاحة";
        _ar["Label_AppVersionUpdates"] = "إصدار التطبيق والتحديثات";
        _ar["Label_WhatsNew"] = "ما الجديد";
        _ar["Button_Refresh"] = "تحديث";
        _ar["Button_Connect"] = "اتصال";
        _ar["Button_Disconnect"] = "قطع الاتصال";
        _ar["Button_Send"] = "إرسال";
        _ar["Button_CheckForUpdates"] = "التحقق من التحديثات";
        _ar["Button_ClearLogs"] = "مسح السجلات";
        _ar["Button_ExportToTxt"] = "تصدير إلى .txt";
        _ar["Status_Connected"] = "متصل";
        _ar["Status_Disconnected"] = "غير متصل";
        _ar["Label_RecommendedForDevice"] = "موصى به للجهاز المتصل";
        _ar["Label_Yes"] = "نعم";
        _ar["Label_No"] = "لا";
        _ar["Format_ModulesCount"] = "{0} وحدات";
        _ar["Language_English"] = "English";
        _ar["Language_Arabic"] = "العربية";
        _ar["Label_Language"] = "اللغة";
        _ar["Logs_Timestamp"] = "الوقت";
        _ar["Logs_Direction"] = "الاتجاه";
        _ar["Logs_Message"] = "الرسالة";
        _ar["Logs_Status"] = "الحالة";
        _ar["Device_NoDevice_Title"] = "لم يتم اكتشاف جهاز";
        _ar["Device_NoDevice_Message"] = "تأكد من توصيل الجهاز واختيار المنفذ الصحيح.";
        _ar["Device_NoDevice_Suggestion_USB"] = "تحقق من توصيل USB";
        _ar["Device_NoDevice_Suggestion_Port"] = "اختر منفذ COM الصحيح";
        _ar["Device_NoDevice_Suggestion_Refresh"] = "انقر على تحديث";
        _ar["Device_NoDevice_Suggestion_Drivers"] = "تثبيت برامج التشغيل المطلوبة";
        _ar["Device_NoDevice_Button_Drivers"] = "تنزيل برامج التشغيل";
        _ar["Device_NoDevice_Button_Support"] = "فتح صفحة الدعم";
        _ar["Device_NoDevice_Button_Troubleshooting"] = "دليل استكشاف الأخطاء";
        _ar["Device_Unsupported_Title"] = "تم اكتشاف جهاز غير مدعوم";
        _ar["Device_Unsupported_Button_RequestSupport"] = "طلب الدعم";
        _ar["Device_Unsupported_Button_Website"] = "فتح الموقع";
        _ar["Settings_Appearance"] = "المظهر";
        _ar["Settings_AppearanceHelp"] = "اختر مظهر التطبيق.";
        _ar["Settings_LanguageHelp"] = "الإنجليزية افتراضية. العربية تدعم من اليمين لليسار.";
        _ar["Settings_AppPreferences"] = "تفضيلات التطبيق";
        _ar["Settings_StartWithWindows"] = "التشغيل مع Windows";
        _ar["Settings_CheckForUpdatesAuto"] = "التحقق من التحديثات تلقائياً";
        _ar["Settings_DiagnosticsMode"] = "وضع التشخيص";
        _ar["Settings_About"] = "حول";
        _ar["Settings_Label_AppName"] = "اسم التطبيق:";
        _ar["Settings_AppName"] = "مركز تحكم MABA";
        _ar["Settings_Version"] = "0.1.0";
        _ar["Settings_Label_Company"] = "الشركة:";
        _ar["Settings_Company"] = "MABA Solutions";
        _ar["Sidebar_Discover"] = "اكتشف";
        _ar["Page_Discover_Title"] = "اكتشف";
        _ar["Page_Discover_Placeholder"] = "المنتجات والوحدات والروابط السريعة.";
        _ar["Discover_FeaturedProducts"] = "المنتجات المميزة";
        _ar["Discover_FeaturedModules"] = "الوحدات المميزة";
        _ar["Discover_QuickActions"] = "إجراءات سريعة";
        _ar["Discover_Product_DexterVP1"] = "MABA Dexter VP1";
        _ar["Discover_Product_Scara"] = "MABA SCARA";
        _ar["Discover_Product_CNC"] = "منصة MABA CNC";
        _ar["Discover_Module_DexterCal"] = "ماكرو باد دكستر";
        _ar["Discover_Module_ScaraControl"] = "تحكم SCARA";
        _ar["Discover_Module_CNCManager"] = "مدير CNC";
        _ar["Discover_Action_Website"] = "فتح موقع MABA";
        _ar["Discover_Action_StartProject"] = "بدء مشروع";
        _ar["Discover_Action_Support"] = "الدعم والتنزيلات";
        _ar["Discover_Product_DexterVP1_Desc"] = "منصة تحكم دقيقة لأنظمة MABA Dexter.";
        _ar["Discover_Product_Scara_Desc"] = "منصة ذراع روبوتية صناعية للحركة والأتمتة.";
        _ar["Discover_Product_CNC_Desc"] = "بيئة متكاملة للتحكم ومراقبة CNC.";
        _ar["Discover_Module_DexterCal_Desc"] = "تعيين مفاتيح الماكرو (أ–ب) لأجهزة Dexter.";
        _ar["Discover_Module_ScaraControl_Desc"] = "أدوات التحكم بالروبوت لأنظمة SCARA.";
        _ar["Discover_Module_CNCManager_Desc"] = "أدوات المراقبة والتحكم لأجهزة CNC.";
        _ar["Discover_Status_Available"] = "متاح";
        _ar["Discover_Status_ComingSoon"] = "قريباً";
        _ar["Cnc_Tab_Operate"] = "تشغيل";
        _ar["Cnc_Tab_Monitor"] = "مراقبة";
        _ar["Cnc_Tab_Setup"] = "إعداد";
    }
}
