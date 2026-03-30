using System.Windows.Controls;
using MabaControlCenter.ViewModels;

namespace MabaControlCenter.Views;

public partial class DexterCalibrationView : UserControl
{
    public DexterCalibrationView()
    {
        InitializeComponent();
        Unloaded += (_, _) =>
        {
            if (DataContext is DexterCalibrationViewModel vm)
                vm.OnViewUnloaded();
        };
    }
}
