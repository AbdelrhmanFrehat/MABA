# Maba Control Center — System Overview for Cursor / Developers

This document explains the full system so another Cursor agent or developer can work on the project without guessing.

---

## 1. What the app is

- **Name:** Maba Control Center  
- **Type:** WPF desktop app (.NET 8, Windows)  
- **Purpose:** Control center for MABA products: devices, modules (e.g. Dexter Calibration), logs, settings, updates, and discovery.  
- **Stack:** C#, WPF, MVVM-style (ViewModels + DataTemplates), no external UI framework.

---

## 2. High-level architecture

- **Shell:** `MainWindow` has a **left sidebar** (navigation) and a **main content area** (header + `ContentControl`).  
- **Navigation:** One `NavigationService` holds all page ViewModels and exposes `CurrentViewModel`. Sidebar buttons call `NavigateTo("PageKey")`.  
- **Content:** The `ContentControl` is bound to `CurrentViewModel`. WPF picks the correct **View** (UserControl) via **DataTemplates** in `MainWindow.xaml` (ViewModel type → View type).  
- **Theme & language:** Applied at startup from persisted settings; theme/language services are used by Settings and by the shell.

**Startup flow:**

1. **App.xaml.cs**  
   - Creates `LocalizationService` and `LocalizedLabels`, stores them in `Application.Current.Resources["LocalizationService"]` and `["Labels"]`.  
   - `Labels` is used across XAML as `{StaticResource Labels}` for localized strings.

2. **MainWindow.xaml.cs**  
   - Creates: `LoggingService`, `DeviceService`, `ModuleService`, `UpdateService`, `NewsService`, `ThemeService`, `SettingsService`.  
   - Gets `ILocalizationService` from app resources.  
   - **Loads settings:** `settingsService.Load()` → applies `themeService.ApplyTheme(settings.Theme)` and `localizationService.SetCulture(settings.Language)`.  
   - Creates `NavigationService(..., themeService, localizationService, settingsService)`, then `MainViewModel(nav)`.  
   - Sets `DataContext = MainViewModel`.  
   - On Loaded, sets `FlowDirection` from localization (for RTL/LTR).

3. **MainViewModel**  
   - Exposes `CurrentViewModel` (from nav), `NavigateCommand` (parameter = page key string), and `IsDexterCalibrationModule` (for showing the “Back to Modules” bar).  
   - On init, navigates to `"Dashboard"`.

4. **NavigationService**  
   - In its constructor, creates **all** page ViewModels once (Dashboard, Discover, Devices, Commands, Modules, DexterCalibration, Logs, Settings) and stores them in a dictionary by page key.  
   - `NavigateTo(pageKey)` sets `CurrentViewModel` to the corresponding ViewModel.  
   - Special case: when navigating to Devices, `DevicesViewModel.LoadPorts()` is called.

---

## 3. Page keys and views

| Page key           | ViewModel               | View (UserControl)     |
|--------------------|-------------------------|------------------------|
| Dashboard          | DashboardViewModel      | DashboardView          |
| Discover           | DiscoverViewModel      | DiscoverView           |
| Devices            | DevicesViewModel       | DevicesView            |
| Commands           | CommandsViewModel      | CommandsView           |
| Modules            | ModulesViewModel       | ModulesView            |
| DexterCalibration  | DexterCalibrationViewModel | DexterCalibrationView |
| Logs               | LogsViewModel          | LogsView               |
| Settings           | SettingsViewModel      | SettingsView           |

DataTemplates in `MainWindow.xaml` map each ViewModel type to its View. The sidebar binds `CommandParameter` to these keys (e.g. `"Settings"`, `"Modules"`).

---

## 4. Important folders and files

- **App.xaml / App.xaml.cs** — Application entry, `LocalizationService` and `Labels` in app resources, `RestartRequested` for language-change restart.  
- **Views/MainWindow.xaml(.cs)** — Shell: sidebar, header, `ContentControl` for current page; DataTemplates; “Back to Modules” bar when `IsDexterCalibrationModule` is true.  
- **ViewModels/MainViewModel.cs** — Root DataContext; navigation and `CurrentViewModel`.  
- **Services/NavigationService.cs** — Creates all page VMs, holds `CurrentViewModel`, implements `NavigateTo`.  
- **Services/** — All services and their interfaces (see below).  
- **Views/*.xaml** — One UserControl per page; bind to corresponding ViewModel (DataContext is set by the DataTemplate when the VM is current).  
- **ViewModels/*.cs** — Page logic; use `RelayCommand` for commands, `ViewModelBase` for INotifyPropertyChanged.  
- **Themes/MabaTheme.xaml** — Default theme (merged in App.xaml); Dark/Light are separate theme XAMLs swapped at runtime.  
- **Localization/LocalizedLabels.cs** — Exposes localized strings for XAML (`Labels.KeyName`).  
- **Services/LocalizationService.cs** — Loads/saves culture (en/ar), provides `GetString(key)`, sets `FlowDirection`.  
- **Models/** — DTOs and small models (e.g. `AppSettings`, `ModuleInfo`, `DeviceProfile`, `LogEntry`).  
- **Converters/** — Value converters for XAML (e.g. bool ↔ Visibility, null ↔ Visibility).

---

## 5. Services (and who uses them)

- **ILocalizationService / LocalizationService** — Culture (en/ar), RTL, and string lookup. Used by App (Labels), MainWindow (FlowDirection), Settings, and many ViewModels for titles/labels.  
- **IThemeService / ThemeService** — Current theme (MABA / Dark / Light); applies merged dictionary. Used by MainWindow at startup and by SettingsViewModel.  
- **ISettingsService / SettingsService** — Load/save `AppSettings` (theme, language, StartWithWindows, CheckForUpdatesAutomatically, DiagnosticsMode) to `%LocalAppData%\MabaControlCenter\settings.json`. Used at startup (MainWindow) and by SettingsViewModel.  
- **INavigationService / NavigationService** — Page dictionary and `NavigateTo`. Used by MainViewModel (sidebar) and by DexterCalibrationViewModel (“Back to Modules”).  
- **IDeviceService / DeviceService** — Device/port logic (e.g. COM). Used by Devices, Commands, Modules, Dashboard.  
- **ILoggingService / LoggingService** — Central log entries. Used by Commands, DeviceService, and LogsViewModel.  
- **IModuleService / ModuleService** — Module list/capabilities. Used by Dashboard, Modules.  
- **IUpdateService / UpdateService**, **INewsService / NewsService** — Updates and news (e.g. Dashboard).  
- **RelayCommand** — ViewModels use this for ICommand; parameter is often the page key or another value.

Theme and culture are also persisted in separate files by ThemeService and LocalizationService (`theme.txt`, `culture.txt`) for backward compatibility; the single source of truth at startup is `settings.json` (theme + language applied from there).

---

## 6. Persistence and startup behavior

- **Settings (app preferences):**  
  - Stored in `%LocalApplicationData%\MabaControlCenter\settings.json` (JSON).  
  - Contains: Theme, Language, StartWithWindows, CheckForUpdatesAutomatically, DiagnosticsMode.  
  - On startup: MainWindow loads via `SettingsService.Load()`, then calls `ThemeService.ApplyTheme(settings.Theme)` and `LocalizationService.SetCulture(settings.Language)`.  
- **Theme:** Also written by ThemeService to `theme.txt` in the same folder when user changes theme.  
- **Language:** Also written by LocalizationService to `culture.txt` when user changes language (app may restart for full RTL/LTR apply).  
- **Settings page:** SettingsViewModel loads settings (for booleans) and on every change (theme, language, checkboxes) calls `SettingsService.Save(...)` with the current state.

---

## 7. UI and theme

- **MABA theme:** Default; defined in `Themes/MabaTheme.xaml` (colors, brushes, SidebarNavButton, etc.). Dark and Light themes are separate XAMLs that replace the first merged dictionary.  
- **Brushes used in views:**  
  - `BackgroundDarkBrush`, `BackgroundLightBrush`, `BackgroundMediumBrush`  
  - `TextPrimaryBrush`, `TextSecondaryBrush`, `TextOnLightBrush`  
  - `BorderBrush`, `AccentBrush`, `SidebarBrush`, `HeaderBrush`  
- **Sidebar:** Buttons use `SidebarNavButton` style; navigation is via `NavigateCommand` with `CommandParameter` set to the page key.  
- **Content area:** Top = header with app title; below = “Back to Modules” bar (only when `IsDexterCalibrationModule`); then `ContentControl` with `Content={Binding CurrentViewModel}` and `VerticalContentAlignment="Top"`.  
- **Localization in XAML:** Most user-visible text uses `{Binding Source={StaticResource Labels}, Path=KeyName}` so language can switch (Labels raises PropertyChanged when culture changes).

---

## 8. Key features by area

- **Dashboard:** Update/news, device/module summary, quick links.  
- **Discover:** Featured products/modules and quick actions (no hardware).  
- **Devices:** COM port list, connect/disconnect, device info.  
- **Commands:** Send commands to device; logging.  
- **Modules:** List of modules (e.g. Dexter Calibration); opening one navigates to `DexterCalibration`.  
- **Dexter Calibration:** Full module UI: connection status, joint sliders (0–180°), position display, Start/Save Calibration and Reset, module log (all simulated, no hardware).  
- **Logs:** List of log entries from LoggingService.  
- **Settings:** Theme, language, StartWithWindows, CheckForUpdatesAutomatically, DiagnosticsMode; persistence via SettingsService. **About** is a large section at the bottom of Settings (MABA Solutions copy: direction, medical focus, engineering discipline, tech capabilities, product philosophy, app name/version/company).

---

## 9. Cursor-specific rules

- **`.cursor/rules/app-restart.mdc`** (always apply):  
  After any code or asset change:  
  1. **Close** the app (kill `MabaControlCenter` process if running).  
  2. **Start** it again with `dotnet run` from the project root (e.g. in background).  
  Do this yourself; do not ask the user to restart.  
- **Build:** From project root: `dotnet build`. If the build fails with “file in use”, the exe is still running — kill the process then rebuild.

---

## 10. Adding a new page

1. Create a ViewModel (e.g. `NewPageViewModel`) and a View (e.g. `NewPageView.xaml` + code-behind).  
2. In `NavigationService` constructor: create the ViewModel (inject any services it needs) and add it to the dictionary, e.g. `_viewModels["NewPage"] = new NewPageViewModel(...)`.  
3. In `MainWindow.xaml`: add a `DataTemplate` mapping `NewPageViewModel` to `NewPageView`.  
4. Add a sidebar button (or other trigger) that calls `NavigateCommand` with `CommandParameter="NewPage"`.

---

## 11. Adding a new setting or localized string

- **Setting:** Add the property to `Models/AppSettings.cs`, read/write it in `SettingsService`, and in `SettingsViewModel` (load in ctor, save in setter via `SaveCurrentSettings()`).  
- **Localized string:** Add the key to `LocalizationService` (LoadEnglish / LoadArabic), then add a property on `LocalizedLabels` that returns `_service.GetString("Key")`. Use in XAML as `Path=KeyName` on `Labels`.

---

This is the full system in one place. Use it to reason about startup, navigation, persistence, theme, and where to add or change behavior.
