using System;
using System.Globalization;
using System.Windows.Data;
using plcdb_lib.Models;
using System.Data;
using plcdb.ViewModels;
using System.Windows;
using plcdb_lib.WCF;
using System.Windows.Media;

namespace plcdb.Converters
{
    [ValueConversion(typeof(String), typeof(SolidColorBrush))]
    public class ObjectStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch ((String)value)
                {
                    case "Error":
                        return new SolidColorBrush(Colors.Red);

                    case "Good":
                        return new SolidColorBrush(Colors.LightGreen);

                    case "NotRunning":
                        return new SolidColorBrush(Colors.Yellow);

                }
                return new SolidColorBrush(Colors.Red);
            }
            catch (Exception e)
            {
                return new SolidColorBrush(Colors.Red);
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
