using GasStationTracker.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GasStationTracker.Controls
{
    /// <summary>
    /// Interaction logic for SessionStatistics.xaml
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
            PopupPlacement placement = (PopupPlacement)PopupPlacementConverter.ConvertBack(UserSettings.Default.SessionStatsPopupPlacement);
            PopupView.CurrentPlacement = placement;
            PopupView.PropertyChanged += (o, e) => SavePopupPlacement();
        }

        private void SavePopupPlacement()
        {
            UserSettings.Default.SessionStatsPopupPlacement = (string)PointerSourceConverter.Convert(PopupView.CurrentPlacement);
        }
    }
}
