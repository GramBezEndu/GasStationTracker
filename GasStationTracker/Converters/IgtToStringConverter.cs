using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace GasStationTracker.Converters
{
    [ValueConversion(typeof(InGameTime), typeof(string))]
    public class IgtToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var igt = (InGameTime)value;
            return string.Format("{0} days {1} hours {2} minutes", igt.Days, igt.Hours, igt.Minutes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
