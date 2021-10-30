using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        public string CurrentMethod 
        {
            get
            {
                return TimingMethods[SelectedMethodIndex];
            }
        }

        public int SelectedMethodIndex { get; set; } = 1;

        private Dictionary<string, CheckBox> checkboxes;

        public LiveGraphs()
        {
            InitializeComponent();
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
            this.ResetSize();
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

        private void expander_Loaded(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if (expander != null)
            {
                expander.PreviewMouseLeftButtonDown += (ss, ee) =>
                {
                    ee.Handled = ee.ClickCount < 1;
                    expander.IsExpanded = true;
                };
                expander.PreviewMouseLeftButtonUp += (ss, ee) =>
                {
                    if (expander.IsExpanded)
                    {
                        var popup = (Popup)expander.Content;
                        var positionRelative = ee.GetPosition(popup);
                        double height = ViewSettings.ActualHeight;
                        bool popupClicked = positionRelative.X >= 0 &&
                            positionRelative.X <= popup.Width &&
                            positionRelative.Y >= 0 &&
                            positionRelative.Y <= height;
                        if (!popupClicked)
                        {
                            ee.Handled = true;
                        }
                    }
                    else
                    {
                        expander.IsExpanded = false;
                        Keyboard.ClearFocus();
                        ee.Handled = true;
                    }
                };
            }
        }
    }
}
