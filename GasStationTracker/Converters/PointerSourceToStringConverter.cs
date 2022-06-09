namespace GasStationTracker.Converters
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Windows.Data;
    using GasStationTracker.Controls;

    [ValueConversion(typeof(PointerSource), typeof(string))]
    public class PointerSourceToStringConverter : IValueConverter
    {
        public static object Convert(object value)
        {
            return new PointerSourceToStringConverter().Convert(value, null, null, CultureInfo.CurrentCulture);
        }

        public static object ConvertBack(object value)
        {
            return new PointerSourceToStringConverter().ConvertBack(value, null, null, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(Regex.Replace(value.ToString(), @"\s", string.Empty), out PointerSource pointerSource);

            // Add spaces to camel case text
            return Regex.Replace(pointerSource.ToString(), "(\\B[A-Z0-9])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(Regex.Replace(value.ToString(), @"\s", string.Empty), out PointerSource pointerSource);
            return pointerSource;
        }
    }
}