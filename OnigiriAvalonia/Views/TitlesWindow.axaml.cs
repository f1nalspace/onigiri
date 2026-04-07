using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Finalspace.Onigiri.ViewModels;

namespace Finalspace.Onigiri.Views;

public partial class TitlesWindow : Window
{
    public TitlesWindow()
    {
        InitializeComponent();
    }

    private void FilterTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        if (DataContext is TitlesViewModel vm)
            vm.StartRefreshTimer();
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
