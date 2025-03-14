using System;
using Microsoft.UI.Xaml.Data;

namespace ClusterOS.Helpers
{
    class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan ts)
            {
                return ts.ToString(@"hh\:mm\:ss");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}