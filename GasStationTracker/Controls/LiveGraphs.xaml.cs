namespace GasStationTracker.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for LiveGraphs.xaml.
    /// </summary>
    public partial class LiveGraphs : UserControl
    {
        private readonly Dictionary<string, CheckBox> checkboxes;

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
            Loaded += new RoutedEventHandler(OnViewLoaded);
            this.ResetSize();
        }

        public List<string> TimingMethods { get; set; } = new List<string>()
        {
            "IGT",
            "Real Time",
        };

        public string CurrentMethod => TimingMethods[SelectedMethodIndex];

        public int SelectedMethodIndex { get; set; } = 1;

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(expander);
            if (window != null)
            {
                window.LocationChanged += (sender2, args) =>
                {
                    double offset = ViewSettingsPopUp.HorizontalOffset;

                    // "bump" the offset to cause the popup to reposition itself on its own
                    ViewSettingsPopUp.HorizontalOffset = offset + 1;
                    ViewSettingsPopUp.HorizontalOffset = offset;
                };

                // Also handle the window being resized (so the popup's position stays
                //  relative to its target element if the target element moves upon
                //  window resize)
                window.SizeChanged += (sender3, e2) =>
                {
                    double offset = ViewSettingsPopUp.HorizontalOffset;
                    ViewSettingsPopUp.HorizontalOffset = offset + 1;
                    ViewSettingsPopUp.HorizontalOffset = offset;
                };
            }
        }

        private void ContextClick(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.IsChecked == true)
            {
                UncheckAllExcept(checkbox.Name);
                Graph.Model.UpdateGraphLineSeries(checkbox.Content.ToString());
            }
            else
            {
                Graph.Model.UpdateGraphLineSeries(string.Empty);
            }
        }

        private void UncheckAllExcept(string name)
        {
            foreach (CheckBox checkbox in checkboxes.Values)
            {
                checkbox.IsChecked = false;
            }

            checkboxes[name].IsChecked = true;
        }

        private void ExpanderLoaded(object sender, RoutedEventArgs e)
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
                        Popup popup = (Popup)expander.Content;
                        Point positionRelative = ee.GetPosition(popup);
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
