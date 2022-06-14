﻿namespace GasStationTracker.Converters
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Windows.Data;
    using GasStationTracker.Controls;

    [ValueConversion(typeof(PopupPlacement), typeof(string))]
    public class PopupPlacementToStringConverter : IValueConverter
    {
        public static object ConvertBack(object value)
        {
            return new PopupPlacementToStringConverter().ConvertBack(value, null, null, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PopupPlacement popupPlacement = (PopupPlacement)value;

            // Add spaces camel case
            return Regex.Replace(popupPlacement.ToString(), "(\\B[A-Z0-9])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(Regex.Replace(value.ToString(), @"\s", string.Empty), out PopupPlacement placement);
            return placement;
        }
    }
}
