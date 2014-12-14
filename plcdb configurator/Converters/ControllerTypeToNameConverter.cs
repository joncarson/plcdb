using System;
using System.Globalization;
using System.Windows.Data;
using plcdb_lib.Models;
using System.Data;
using plcdb.ViewModels;
using System.Windows;
using plcdb_lib.Models.Controllers;
using System.Linq;
using System.Reflection;

namespace plcdb.Converters
{
    [ValueConversion(typeof(Type), typeof(String))]
    public class ControllerTypeToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Searches type for a public static property named "Name", otherwise uses class name
            Type t = (Type)value;
            try
            {
                var nameProperties = t.GetProperties().Where(p => p.Name == "Name");
                if (nameProperties.Count() > 0)
                {
                    return nameProperties.First().GetValue(null, null);
                }
                else
                {
                    return t.Name;
                }
            }
            catch (Exception e)
            {
                return t.Name;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
