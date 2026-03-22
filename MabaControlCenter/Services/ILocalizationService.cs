namespace MabaControlCenter.Services;

public interface ILocalizationService
{
    string CurrentCulture { get; }
    System.Windows.FlowDirection FlowDirection { get; }
    void SetCulture(string cultureCode);
    string GetString(string key);
    event EventHandler? CultureChanged;
}
