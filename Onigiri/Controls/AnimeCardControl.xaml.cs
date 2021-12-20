using DevExpress.Mvvm;
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
    /// Interaction logic for AnimeCardControl.xaml
    /// </summary>
    public partial class AnimeCardControl : UserControl
    {
        public DelegateCommand<ScrollViewer> ScrollPrevTagCommand { get; }
        public DelegateCommand<ScrollViewer> ScrollNextTagCommand { get; }

        private const double TagButtonWidth = 78.0;

        public AnimeCardControl()
        {
            InitializeComponent();
            ScrollPrevTagCommand = new DelegateCommand<ScrollViewer>(ScrollPrevTag, CanScrollPrevTag);
            ScrollNextTagCommand = new DelegateCommand<ScrollViewer>(ScrollNextTag, CanScrollNextTag);
        }

        private bool CanScrollPrevTag(ScrollViewer scrollViewer) => scrollViewer.HorizontalOffset > 0;

        private void ScrollPrevTag(ScrollViewer scrollViewer)
        {
            double newOffset = scrollViewer.HorizontalOffset - TagButtonWidth;
            scrollViewer.ScrollToHorizontalOffset(newOffset);
        }

        private bool CanScrollNextTag(ScrollViewer scrollViewer) => scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth;

        private void ScrollNextTag(ScrollViewer scrollViewer)
        {
            double newOffset = scrollViewer.HorizontalOffset + TagButtonWidth;
            scrollViewer.ScrollToHorizontalOffset(newOffset);
        }
    }
}
