﻿using Finalspace.Onigiri.ViewModels;
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
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void closeButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ConfigViewModel configViewModel)
            {
                configViewModel.CmdApply.Execute(null);

                DialogResult = true;

                Close();
            }
        }
    }
}
