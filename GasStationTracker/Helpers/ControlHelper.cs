namespace GasStationTracker.Controls
{
    using System.ComponentModel;
    using System.Windows.Controls;

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
