using Avalonia.Controls;
using Avalonia.Interactivity;
using Finalspace.Onigiri.ViewModels;

namespace Finalspace.Onigiri.Views;

public partial class ConfigWindow : Window
{
    public ConfigWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ConfigViewModel configViewModel)
        {
            configViewModel.CmdApply.Execute(null);
            Close(true);
        }
    }
}
