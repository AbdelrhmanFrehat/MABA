using System.Windows.Input;
using MabaControlCenter.Services;

namespace MabaControlCenter.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    public MainViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.CurrentViewModelChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(IsDexterCalibrationModule));
        };
        NavigateCommand = new RelayCommand(Navigate);
        _navigation.NavigateTo("Dashboard");
    }

    public object? CurrentViewModel => _navigation.CurrentViewModel;

    /// <summary>True when the main content is the Dexter Calibration module (show back bar).</summary>
    public bool IsDexterCalibrationModule => CurrentViewModel is DexterCalibrationViewModel;

    public ICommand NavigateCommand { get; }

    private void Navigate(object? parameter)
    {
        if (parameter is string pageKey)
            _navigation.NavigateTo(pageKey);
    }
}
