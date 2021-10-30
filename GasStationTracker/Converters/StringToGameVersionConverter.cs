using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace GasStationTracker.Converters
{
    [ValueConversion(typeof(string), typeof(Version))]
    public class StringToGameVersionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value.ToString();
            return new Version(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Version version = (Version)value;
            return (string)version.ToString();
        }
    }
}
