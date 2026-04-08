using Avalonia;
using Avalonia.Controls;

namespace Finalspace.Onigiri.Controls;

public partial class LoadingBarControl : UserControl
{
    public static readonly StyledProperty<string> LoadingSubjectProperty =
        AvaloniaProperty.Register<LoadingBarControl, string>(nameof(LoadingSubject));

    public string LoadingSubject
    {
        get => GetValue(LoadingSubjectProperty);
        set => SetValue(LoadingSubjectProperty, value);
    }

    public static readonly StyledProperty<string> LoadingHeaderProperty =
        AvaloniaProperty.Register<LoadingBarControl, string>(nameof(LoadingHeader));

    public string LoadingHeader
    {
        get => GetValue(LoadingHeaderProperty);
        set => SetValue(LoadingHeaderProperty, value);
    }

    public static readonly StyledProperty<double> LoadingPercentageProperty =
        AvaloniaProperty.Register<LoadingBarControl, double>(nameof(LoadingPercentage));

    public double LoadingPercentage
    {
        get => GetValue(LoadingPercentageProperty);
        set => SetValue(LoadingPercentageProperty, value);
    }

    public static readonly StyledProperty<bool> IsLoadingMarqueProperty =
        AvaloniaProperty.Register<LoadingBarControl, bool>(nameof(IsLoadingMarque));

    public bool IsLoadingMarque
    {
        get => GetValue(IsLoadingMarqueProperty);
        set => SetValue(IsLoadingMarqueProperty, value);
    }

    public LoadingBarControl()
    {
        InitializeComponent();
    }
}
