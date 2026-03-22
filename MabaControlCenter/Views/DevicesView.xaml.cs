using System.Windows.Controls;
using MabaControlCenter.ViewModels;

namespace MabaControlCenter.Views;

public partial class DevicesView : UserControl
{
    public DevicesView()
    {
        InitializeComponent();
    }

    private void DevicesView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Defer so DataContext and bindings are ready
        Dispatcher.BeginInvoke(() =>
        {
            if (DataContext is DevicesViewModel vm)
                vm.LoadPorts();
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }
}
