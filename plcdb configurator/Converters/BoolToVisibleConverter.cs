﻿using System;
using System.Globalization;
using System.Windows.Data;
using plcdb_lib.Models;
using System.Data;
using plcdb.ViewModels;
using System.Windows;

namespace plcdb.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Hidden)
                return false;
            return true;
        }
    }

   
}
