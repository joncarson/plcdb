using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using plcdb_lib.constants;

namespace plcdb.Converters
{
   
    public class TitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            String OriginalTitle = (string)parameter;
            String ModelPath = values[0] == null ? "" : " - " + (String)values[0];
            String ModelChanged = (bool)values[1] ? "*" : "";
            DateTime LicenseStartTime = values[2] == null ? DateTime.MaxValue : (DateTime)values[2];
            String DemoMode = "";
            if (LicenseStartTime != DateTime.MaxValue && LicenseStartTime != DateTime.MinValue)
            {
                TimeSpan DemoTimeRemaining = CONSTANTS.DemoTimeout - (DateTime.Now - LicenseStartTime);
                if (DemoTimeRemaining.TotalMilliseconds > 0)
                    DemoMode = " (DEMO MODE: " + DemoTimeRemaining.ToString(@"hh\:mm\:ss") + " remaining)";
                else
                    DemoMode = " (DEMO MODE EXPIRED)";
            }
            return OriginalTitle + ModelPath + ModelChanged + DemoMode;
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}