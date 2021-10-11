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

        public LiveGraphs()
        {
            InitializeComponent();
            this.ResetSize();
            TimingMethodsList.ItemsSource = TimingMethods;
        }
    }
}
