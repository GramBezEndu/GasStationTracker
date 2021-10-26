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
    public enum PointerSource
    {
        OnlineRepository = 0,
        EmbeddedInApplication = 1,
        LocalCheatTable = 2,
    };

    /// <summary>
    /// Interaction logic for PointerSettings.xaml
    /// </summary>
    public partial class PointerSettings : UserControl
    {
        public PointerSource CurrentPointerSource { get; set; }

        public PointerSettings()
        {
            InitializeComponent();
            this.ResetSize();
        }

        private void RadioButton_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
