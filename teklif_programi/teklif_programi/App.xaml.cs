using System.Configuration;
using System.Data;
using System.Windows;

namespace teklif_programi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Beklenmeyen bir hata oluştu:\n" + e.Exception.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }

}
