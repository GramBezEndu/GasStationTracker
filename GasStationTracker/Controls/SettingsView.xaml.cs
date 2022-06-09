namespace GasStationTracker.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            Loaded += LoadSettings;
            this.ResetSize();
        }

        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            AutoUpdate.IsChecked = UserSettings.Default.AutoUpdate;
        }
    }
}
