namespace MabaControlCenter.Services;

public interface INavigationService
{
    void NavigateTo(string pageKey);
    object? CurrentViewModel { get; }
    event EventHandler? CurrentViewModelChanged;
}
