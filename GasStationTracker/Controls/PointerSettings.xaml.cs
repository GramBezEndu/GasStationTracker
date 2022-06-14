namespace GasStationTracker.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public enum PointerSource
    {
        OnlineRepository = 0,
        EmbeddedInApplication = 1,
        LocalCheatTable = 2,
    }

    /// <summary>
    /// Interaction logic for PointerSettings.xaml.
    /// </summary>
    public partial class PointerSettings : UserControl
    {
        public PointerSettings()
        {
            InitializeComponent();
            this.ResetSize();
        }

        public PointerSource CurrentPointerSource { get; set; }

        private void RadioButton_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
