namespace GasStationTracker.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(InGameTime), typeof(string))]
    public class IgtToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            InGameTime igt = (InGameTime)value;
            return string.Format("{0} days {1} hours {2} minutes", igt.Days, igt.Hours, igt.Minutes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
