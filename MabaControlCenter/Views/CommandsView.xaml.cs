using System.Windows.Controls;
using System.Windows.Input;
using MabaControlCenter.ViewModels;

namespace MabaControlCenter.Views;

public partial class CommandsView : UserControl
{
    public CommandsView()
    {
        InitializeComponent();
    }

    private void CommandTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        if (DataContext is CommandsViewModel vm && vm.SendCommandCommand.CanExecute(null))
            vm.SendCommandCommand.Execute(null);
    }
}
