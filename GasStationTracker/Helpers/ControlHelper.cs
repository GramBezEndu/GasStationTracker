using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace GasStationTracker.Controls
{
    public static class ControlHelper
    {
        public static void ResetSize(this UserControl userControl)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                userControl.Width = double.NaN;
                userControl.Height = double.NaN;
            }
        }
    }
}
