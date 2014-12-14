using System;
using System.Globalization;
using System.Windows.Data;
using plcdb_lib.Models;
using System.Data;
using plcdb.ViewModels;
using System.Windows;

namespace plcdb.Converters
{
    [ValueConversion(typeof(String), typeof(String))]
    public class StringPrefixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                String Parameter = (String)parameter;
                String Value = (String)value;
                if (Value == null || Value == String.Empty)
                    return parameter;
                else
                {
                    return parameter + " - " + Value;
                }
            }
            catch (Exception e)
            {
                return parameter;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Hidden)
                return true;
            return false;
        }
    }
}
