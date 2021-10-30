using GasStationTracker.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace GasStationTracker.Converters
{
    public class PointerSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PointerSource pointerSource;
            Enum.TryParse(Regex.Replace(value.ToString(), @"\s", ""), out pointerSource);
            //Add spaces camel case
            return Regex.Replace(pointerSource.ToString(), "(\\B[A-Z0-9])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PointerSource pointerSource;
            Enum.TryParse(Regex.Replace(value.ToString(), @"\s", ""), out pointerSource);
            return pointerSource;
        }

        public static object Convert(object value)
        {
            return new PointerSourceConverter().Convert(value, null, null, CultureInfo.CurrentCulture);
        }

        public static object ConvertBack(object value)
        {
            return new PointerSourceConverter().ConvertBack(value, null, null, CultureInfo.CurrentCulture);
        }
    }
}