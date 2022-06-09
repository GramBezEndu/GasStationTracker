namespace GasStationTracker
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                "An unhandled exception just occurred: " + e.Exception.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}
