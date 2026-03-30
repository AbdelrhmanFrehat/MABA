using System.Diagnostics;
using System.Windows.Input;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class DiscoverViewModel : ViewModelBase
{
    private const string MabaWebsiteUrl = "https://mabasol.com/";
    private const string MabaProjectsUrl = "https://mabasol.com/projects";
    private const string MabaSupportUrl = "https://mabasol.com/software";

    private readonly ILocalizationService _localizationService;

    public DiscoverViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(PlaceholderText));
        };
        OpenUrlCommand = new RelayCommand(OpenUrl);
    }

    public string Title => _localizationService.GetString("Page_Discover_Title");
    public string PlaceholderText => _localizationService.GetString("Page_Discover_Placeholder");

    public ICommand OpenUrlCommand { get; }

    public string MabaWebsiteUrlValue => MabaWebsiteUrl;
    public string MabaProjectsUrlValue => MabaProjectsUrl;
    public string MabaSupportUrlValue => MabaSupportUrl;

    private static void OpenUrl(object? parameter)
    {
        if (parameter is not string url || string.IsNullOrWhiteSpace(url)) return;
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
        catch { /* ignore */ }
    }
}
