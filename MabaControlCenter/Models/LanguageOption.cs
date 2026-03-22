using System.ComponentModel;
using MabaControlCenter.Services;

namespace MabaControlCenter.Models;

public class LanguageOption : INotifyPropertyChanged
{
    private readonly ILocalizationService _service;
    private readonly string _displayKey;

    public LanguageOption(string cultureCode, string displayKey, ILocalizationService service)
    {
        CultureCode = cultureCode;
        _displayKey = displayKey;
        _service = service;
        _service.CultureChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
    }

    public string CultureCode { get; }
    public string DisplayName => _service.GetString(_displayKey);

    public event PropertyChangedEventHandler? PropertyChanged;
}
