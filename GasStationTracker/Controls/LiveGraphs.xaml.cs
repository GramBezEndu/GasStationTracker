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
        }

        private void ContextClick(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender as CheckBox;
            if (checkbox.IsChecked == true)
            {
                UncheckAllExcept(checkbox.Name);
                Graph.Model.UpdateGraphLineSeries(checkbox.Name);
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
