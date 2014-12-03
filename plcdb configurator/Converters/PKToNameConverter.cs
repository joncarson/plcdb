using System;
using System.Globalization;
using System.Windows.Data;
using plcdb_lib.Models;
using System.Data;
using plcdb.ViewModels;

namespace plcdb.Converters
{
    [ValueConversion(typeof(int), typeof(String))]
    public class PKToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                String TableType = (string)parameter;
                MainWindowViewModel vm = App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel;
                var rows = (vm.ActiveModel.Tables[TableType].Select("PK=" + value.ToString()));
                DataRow row = rows[0];
                return row["Name"];
            }
            catch (Exception e)
            {
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool original = (bool)value;
            return !original;
        }
    }
}
