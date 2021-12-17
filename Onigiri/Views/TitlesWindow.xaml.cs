using Finalspace.Onigiri.ViewModels;
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
using System.Windows.Shapes;

namespace Finalspace.Onigiri.Views
{
    /// <summary>
    /// Interaction logic for TitlesWindow.xaml
    /// </summary>
    public partial class TitlesWindow : Window
    {
        public TitlesWindow()
        {
            InitializeComponent();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            TitlesViewModel vm = (TitlesViewModel)DataContext;
            vm.StartRefreshTimer();
        }
    }
}
