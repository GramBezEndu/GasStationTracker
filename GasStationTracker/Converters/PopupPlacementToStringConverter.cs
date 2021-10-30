using GasStationTracker.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace GasStationTracker.Converters
{
    [ValueConversion(typeof(PopupPlacement), typeof(string))]
    public class PopupPlacementToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PopupPlacement popupPlacement = (PopupPlacement)value;
            //Add spaces camel case
            return Regex.Replace(popupPlacement.ToString(), "(\\B[A-Z0-9])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(Regex.Replace(value.ToString(), @"\s", ""), out PopupPlacement placement);
            return placement;
        }

        public static object ConvertBack(object value)
        {
            return new PopupPlacementToStringConverter().ConvertBack(value, null, null, CultureInfo.CurrentCulture);
        }
    }
}
