using System;
using System.Collections.Generic;
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
    /// Interaction logic for LiveGraphs.xaml
    /// </summary>
    public partial class LiveGraphs : UserControl
    {
        public List<string> TimingMethods { get; set; } = new List<string>()
        {
            "IGT",
            "Real Time",
        };

        public string CurrentMethod { get; set; } = "IGT";

        Dictionary<string, CheckBox> checkboxes;

        public LiveGraphs()
        {
            InitializeComponent();
            this.ResetSize();
            TimingMethodsList.ItemsSource = TimingMethods;
            checkboxes = new Dictionary<string, CheckBox>()
            {
                { checkbox0.Name, checkbox0 },
                { checkbox1.Name, checkbox1 },
                { checkbox2.Name, checkbox2 },
                { checkbox3.Name, checkbox3 },
                { checkbox4.Name, checkbox4 },
            };
            this.Loaded += new RoutedEventHandler(OnViewLoaded);
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(expander);
            // w should not be Null now!
            if (null != w)
            {
                w.LocationChanged += delegate (object sender2, EventArgs args)
                {
                    var offset = ViewSettingsPopUp.HorizontalOffset;
                    // "bump" the offset to cause the popup to reposition itself
                    //   on its own
                    ViewSettingsPopUp.HorizontalOffset = offset + 1;
                    ViewSettingsPopUp.HorizontalOffset = offset;
                };
                // Also handle the window being resized (so the popup's position stays
                //  relative to its target element if the target element moves upon 
                //  window resize)
                w.SizeChanged += delegate (object sender3, SizeChangedEventArgs e2)
                {
                    var offset = ViewSettingsPopUp.HorizontalOffset;
                    ViewSettingsPopUp.HorizontalOffset = offset + 1;
                    ViewSettingsPopUp.HorizontalOffset = offset;
                };
            }
        }

        private void ContextClick(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender as CheckBox;
            if (checkbox.IsChecked == true)
            {
                UncheckAllExcept(checkbox.Name);
                Graph.Model.UpdateGraphLineSeries(checkbox.Content.ToString());
            }
            else
            {
                Graph.Model.UpdateGraphLineSeries(String.Empty);
            }
        }

        private void UncheckAllExcept(string name)
        {
            foreach(var checkbox in checkboxes.Values)
            {
                checkbox.IsChecked = false;
            }
            checkboxes[name].IsChecked = true;
        }
    }
}
