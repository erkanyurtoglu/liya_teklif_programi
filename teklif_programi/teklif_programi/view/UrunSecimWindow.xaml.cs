using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class UrunSecimWindow : Window
    {
        public UrunSecimWindow()
        {
            InitializeComponent();
            DataContext = new UrunSecimViewModel(this);
        }

        private void OnlyAllowNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}