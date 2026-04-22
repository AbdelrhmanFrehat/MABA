using System.Windows.Controls;
using MabaControlCenter.ViewModels;

namespace MabaControlCenter.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void PasswordInput_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && sender is PasswordBox box)
            vm.Password = box.Password;
    }
}
