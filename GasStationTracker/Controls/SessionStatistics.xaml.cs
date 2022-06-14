namespace GasStationTracker.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using GasStationTracker.Converters;

    /// <summary>
    /// Interaction logic for SessionStatistics.xaml.
    /// </summary>
    public partial class SessionStatistics : UserControl
    {
        public SessionStatistics()
        {
            InitializeComponent();
            this.ResetSize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PopupView.PopupExpander.IsExpanded = false;
            PopupView.BindingListNames = new string[]
            {
                SessionTimeDisplay.Name,
                IgtPassedDisplay.Name,
                CashEarnedDisplay.Name,
                PopularityGainedDisplay.Name,
            };
            PopupView.PopupContent = Stats;
            PopupPlacement placement = (PopupPlacement)PopupPlacementToStringConverter.ConvertBack(UserSettings.Default.SessionStatsPopupPlacement);
            PopupView.CurrentPlacement = placement;
            PopupView.PropertyChanged += (o, e) => SavePopupPlacement();
        }

        private void SavePopupPlacement()
        {
            UserSettings.Default.SessionStatsPopupPlacement = (string)PointerSourceToStringConverter.Convert(PopupView.CurrentPlacement);
        }
    }
}
