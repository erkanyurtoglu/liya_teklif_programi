using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;                 // <- eklendi
using teklif_programi.Models;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class AlinanTeklifDetayWindow : Window
    {
        private static readonly Regex _intRegex = new(@"^\d*$", RegexOptions.Compiled);

        public AlinanTeklifDetayWindow(Teklif teklif)
        {
            InitializeComponent();
            DataContext = new TeklifDetayViewModel(teklif);
        }

        private void OnlyAllowNumbers(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox tb) return;

            var proposed = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                                  .Insert(tb.SelectionStart, e.Text);

            e.Handled = !_intRegex.IsMatch(proposed);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // İhtiyaç halinde doldurulabilir
        }

        private void SatisSozlesmesi_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TeklifDetayViewModel vm)
            {
                var window = new SatisSozlesmesiWindow(vm);
                window.ShowDialog();
            }
        }

        // --- Tek tıkla hücre düzenleme (XAML'de EventSetter ile bağla) ---
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridCell cell || cell.IsEditing || cell.IsReadOnly) return;

            if (!cell.IsFocused) cell.Focus();

            var dg = FindParent<DataGrid>(cell);
            if (dg != null && dg.SelectionUnit == DataGridSelectionUnit.FullRow)
            {
                var row = FindParent<DataGridRow>(cell);
                if (row != null && !row.IsSelected) row.IsSelected = true;
            }
            dg?.BeginEdit(e);
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent is not null && parent is not T)
                parent = VisualTreeHelper.GetParent(parent);
            return parent as T;
        }
    }
}
