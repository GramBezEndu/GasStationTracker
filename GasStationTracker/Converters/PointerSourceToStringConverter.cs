using GasStationTracker.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace GasStationTracker.Converters
{
    [ValueConversion(typeof(PointerSource), typeof(string))]
    public class PointerSourceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(Regex.Replace(value.ToString(), @"\s", ""), out PointerSource pointerSource);
            //Add spaces camel case
            return Regex.Replace(pointerSource.ToString(), "(\\B[A-Z0-9])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(Regex.Replace(value.ToString(), @"\s", ""), out PointerSource pointerSource);
            return pointerSource;
        }

        public static object Convert(object value)
        {
            return new PointerSourceToStringConverter().Convert(value, null, null, CultureInfo.CurrentCulture);
        }

        public static object ConvertBack(object value)
        {
            return new PointerSourceToStringConverter().ConvertBack(value, null, null, CultureInfo.CurrentCulture);
        }
    }
}