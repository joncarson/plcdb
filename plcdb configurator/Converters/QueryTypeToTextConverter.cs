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
    public class QueryTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((String)value)
            {
                case "SELECT":
                    return "Query Text:";
                case "INSERT":
                    return "Table: ";
                case "UPDATE":
                    return "Table: ";
                case "DELETE":
                    return "Table";
            }
            return "Query Text:";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Hidden)
                return true;
            return false;
        }
    }
}
