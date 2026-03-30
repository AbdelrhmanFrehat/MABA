using System.ComponentModel;
using System.Runtime.CompilerServices;
using MabaControlCenter.Services;

namespace MabaControlCenter.Localization;

public class LocalizedLabels : INotifyPropertyChanged
{
    private readonly ILocalizationService _service;

    public LocalizedLabels(ILocalizationService service)
    {
        _service = service;
        _service.CultureChanged += (_, _) => RaiseAllProperties();
    }

    public string Sidebar_Dashboard => _service.GetString("Sidebar_Dashboard");
    public string Sidebar_Devices => _service.GetString("Sidebar_Devices");
    public string Sidebar_Commands => _service.GetString("Sidebar_Commands");
    public string Sidebar_Modules => _service.GetString("Sidebar_Modules");
    public string Sidebar_Logs => _service.GetString("Sidebar_Logs");
    public string Sidebar_Settings => _service.GetString("Sidebar_Settings");
    public string App_Title => _service.GetString("App_Title");
    public string Page_Dashboard_Title => _service.GetString("Page_Dashboard_Title");
    public string Page_Dashboard_Placeholder => _service.GetString("Page_Dashboard_Placeholder");
    public string Page_Devices_Title => _service.GetString("Page_Devices_Title");
    public string Page_Devices_Placeholder => _service.GetString("Page_Devices_Placeholder");
    public string Page_Commands_Title => _service.GetString("Page_Commands_Title");
    public string Page_Commands_Placeholder => _service.GetString("Page_Commands_Placeholder");
    public string Page_Modules_Title => _service.GetString("Page_Modules_Title");
    public string Page_Modules_Placeholder => _service.GetString("Page_Modules_Placeholder");
    public string Page_Logs_Title => _service.GetString("Page_Logs_Title");
    public string Page_Logs_Placeholder => _service.GetString("Page_Logs_Placeholder");
    public string Page_Settings_Title => _service.GetString("Page_Settings_Title");
    public string Page_Settings_Placeholder => _service.GetString("Page_Settings_Placeholder");
    public string Page_DexterCalibration_Title => _service.GetString("Page_DexterCalibration_Title");
    public string Page_DexterCalibration_Placeholder => _service.GetString("Page_DexterCalibration_Placeholder");
    public string Button_BackToModules => _service.GetString("Button_BackToModules");
    public string Label_ComPort => _service.GetString("Label_ComPort");
    public string Label_Status => _service.GetString("Label_Status");
    public string Label_Theme => _service.GetString("Label_Theme");
    public string Label_Output => _service.GetString("Label_Output");
    public string Label_DeviceInfo => _service.GetString("Label_DeviceInfo");
    public string Label_NoDeviceDetected => _service.GetString("Label_NoDeviceDetected");
    public string Label_ProductName => _service.GetString("Label_ProductName");
    public string Label_ProductCode => _service.GetString("Label_ProductCode");
    public string Label_FirmwareVersion => _service.GetString("Label_FirmwareVersion");
    public string Label_SerialNumber => _service.GetString("Label_SerialNumber");
    public string Label_ConnectionType => _service.GetString("Label_ConnectionType");
    public string Label_RecommendedModule => _service.GetString("Label_RecommendedModule");
    public string Label_SupportStatus => _service.GetString("Label_SupportStatus");
    public string Label_Version => _service.GetString("Label_Version");
    public string Label_Installed => _service.GetString("Label_Installed");
    public string Label_Enabled => _service.GetString("Label_Enabled");
    public string Label_CurrentVersion => _service.GetString("Label_CurrentVersion");
    public string Label_LatestVersion => _service.GetString("Label_LatestVersion");
    public string Label_ConnectedDevice => _service.GetString("Label_ConnectedDevice");
    public string Label_RecommendedModuleSection => _service.GetString("Label_RecommendedModuleSection");
    public string Label_AvailableModules => _service.GetString("Label_AvailableModules");
    public string Label_AppVersionUpdates => _service.GetString("Label_AppVersionUpdates");
    public string Label_WhatsNew => _service.GetString("Label_WhatsNew");
    public string Button_Refresh => _service.GetString("Button_Refresh");
    public string Button_Connect => _service.GetString("Button_Connect");
    public string Button_Disconnect => _service.GetString("Button_Disconnect");
    public string Button_Send => _service.GetString("Button_Send");
    public string Button_CheckForUpdates => _service.GetString("Button_CheckForUpdates");
    public string Button_ClearLogs => _service.GetString("Button_ClearLogs");
    public string Button_ExportToTxt => _service.GetString("Button_ExportToTxt");
    public string Status_Connected => _service.GetString("Status_Connected");
    public string Status_Disconnected => _service.GetString("Status_Disconnected");
    public string Label_RecommendedForDevice => _service.GetString("Label_RecommendedForDevice");
    public string Label_Yes => _service.GetString("Label_Yes");
    public string Label_No => _service.GetString("Label_No");
    public string Language_English => _service.GetString("Language_English");
    public string Language_Arabic => _service.GetString("Language_Arabic");
    public string Format_ModulesCount => _service.GetString("Format_ModulesCount");
    public string Label_Language => _service.GetString("Label_Language");
    public string Logs_Timestamp => _service.GetString("Logs_Timestamp");
    public string Logs_Direction => _service.GetString("Logs_Direction");
    public string Logs_Message => _service.GetString("Logs_Message");
    public string Logs_Status => _service.GetString("Logs_Status");
    public string Device_NoDevice_Title => _service.GetString("Device_NoDevice_Title");
    public string Device_NoDevice_Message => _service.GetString("Device_NoDevice_Message");
    public string Device_NoDevice_Suggestion_USB => _service.GetString("Device_NoDevice_Suggestion_USB");
    public string Device_NoDevice_Suggestion_Port => _service.GetString("Device_NoDevice_Suggestion_Port");
    public string Device_NoDevice_Suggestion_Refresh => _service.GetString("Device_NoDevice_Suggestion_Refresh");
    public string Device_NoDevice_Suggestion_Drivers => _service.GetString("Device_NoDevice_Suggestion_Drivers");
    public string Device_NoDevice_Button_Drivers => _service.GetString("Device_NoDevice_Button_Drivers");
    public string Device_NoDevice_Button_Support => _service.GetString("Device_NoDevice_Button_Support");
    public string Device_NoDevice_Button_Troubleshooting => _service.GetString("Device_NoDevice_Button_Troubleshooting");
    public string Device_Unsupported_Title => _service.GetString("Device_Unsupported_Title");
    public string Device_Unsupported_Button_RequestSupport => _service.GetString("Device_Unsupported_Button_RequestSupport");
    public string Device_Unsupported_Button_Website => _service.GetString("Device_Unsupported_Button_Website");
    public string Settings_Appearance => _service.GetString("Settings_Appearance");
    public string Settings_AppearanceHelp => _service.GetString("Settings_AppearanceHelp");
    public string Settings_LanguageHelp => _service.GetString("Settings_LanguageHelp");
    public string Settings_AppPreferences => _service.GetString("Settings_AppPreferences");
    public string Settings_StartWithWindows => _service.GetString("Settings_StartWithWindows");
    public string Settings_CheckForUpdatesAuto => _service.GetString("Settings_CheckForUpdatesAuto");
    public string Settings_DiagnosticsMode => _service.GetString("Settings_DiagnosticsMode");
    public string Settings_About => _service.GetString("Settings_About");
    public string Settings_Label_AppName => _service.GetString("Settings_Label_AppName");
    public string Settings_AppName => _service.GetString("Settings_AppName");
    public string Settings_Version => _service.GetString("Settings_Version");
    public string Settings_Label_Company => _service.GetString("Settings_Label_Company");
    public string Settings_Company => _service.GetString("Settings_Company");
    public string Sidebar_Discover => _service.GetString("Sidebar_Discover");
    public string Page_Discover_Title => _service.GetString("Page_Discover_Title");
    public string Page_Discover_Placeholder => _service.GetString("Page_Discover_Placeholder");
    public string Discover_FeaturedProducts => _service.GetString("Discover_FeaturedProducts");
    public string Discover_FeaturedModules => _service.GetString("Discover_FeaturedModules");
    public string Discover_QuickActions => _service.GetString("Discover_QuickActions");
    public string Discover_Product_DexterVP1 => _service.GetString("Discover_Product_DexterVP1");
    public string Discover_Product_Scara => _service.GetString("Discover_Product_Scara");
    public string Discover_Product_CNC => _service.GetString("Discover_Product_CNC");
    public string Discover_Module_DexterCal => _service.GetString("Discover_Module_DexterCal");
    public string Discover_Module_ScaraControl => _service.GetString("Discover_Module_ScaraControl");
    public string Discover_Module_CNCManager => _service.GetString("Discover_Module_CNCManager");
    public string Discover_Action_Website => _service.GetString("Discover_Action_Website");
    public string Discover_Action_StartProject => _service.GetString("Discover_Action_StartProject");
    public string Discover_Action_Support => _service.GetString("Discover_Action_Support");
    public string Discover_Product_DexterVP1_Desc => _service.GetString("Discover_Product_DexterVP1_Desc");
    public string Discover_Product_Scara_Desc => _service.GetString("Discover_Product_Scara_Desc");
    public string Discover_Product_CNC_Desc => _service.GetString("Discover_Product_CNC_Desc");
    public string Discover_Module_DexterCal_Desc => _service.GetString("Discover_Module_DexterCal_Desc");
    public string Discover_Module_ScaraControl_Desc => _service.GetString("Discover_Module_ScaraControl_Desc");
    public string Discover_Module_CNCManager_Desc => _service.GetString("Discover_Module_CNCManager_Desc");
    public string Discover_Status_Available => _service.GetString("Discover_Status_Available");
    public string Discover_Status_ComingSoon => _service.GetString("Discover_Status_ComingSoon");

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaiseAllProperties()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
}
