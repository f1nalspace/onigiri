using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Finalspace.Onigiri.Controls
{
    /// <summary>
    /// Interaction logic for LoadingBarControl.xaml
    /// </summary>
    public partial class LoadingBarControl : UserControl
    {
        public static readonly DependencyProperty LoadingSubjectProperty = 
            DependencyProperty.Register(nameof(LoadingSubject), typeof(string), typeof(LoadingBarControl), new PropertyMetadata(defaultValue: null));

        public string LoadingSubject
        {
            get => GetValue(LoadingSubjectProperty) as string;
            set => SetCurrentValue(LoadingSubjectProperty, value as string);
        }

        public static readonly DependencyProperty LoadingHeaderProperty =
            DependencyProperty.Register(nameof(LoadingHeader), typeof(string), typeof(LoadingBarControl), new PropertyMetadata(defaultValue: null));

        public string LoadingHeader
        {
            get => GetValue(LoadingHeaderProperty) as string;
            set => SetCurrentValue(LoadingHeaderProperty, value as string);
        }

        public static readonly DependencyProperty LoadingPercentageProperty =
            DependencyProperty.Register(nameof(LoadingPercentage), typeof(double), typeof(LoadingBarControl), new PropertyMetadata(defaultValue: 0.0));

        public double LoadingPercentage
        {
            get => (double)GetValue(LoadingPercentageProperty);
            set => SetCurrentValue(LoadingPercentageProperty, (double)value);
        }

        public static readonly DependencyProperty IsLoadingMarqueProperty =
            DependencyProperty.Register(nameof(IsLoadingMarque), typeof(bool), typeof(LoadingBarControl), new PropertyMetadata(defaultValue: false));
        public bool IsLoadingMarque
        {
            get => (bool)GetValue(IsLoadingMarqueProperty);
            set => SetCurrentValue(IsLoadingMarqueProperty, (bool)value);
        }

        public LoadingBarControl()
        {
            InitializeComponent();
        }
    }
}
