namespace GasStationTracker.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class IsPointerSourceSelected : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currectSource = UserSettings.Default.PointerSource;
            if (parameter.ToString() == currectSource)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
