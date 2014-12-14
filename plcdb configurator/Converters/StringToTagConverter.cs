using System;
using System.Globalization;
using System.Windows.Data;
using plcdb_lib.Models;
using System.Data;
using plcdb.ViewModels;
using System.Windows;
using System.Text.RegularExpressions;
using System.Linq;

namespace plcdb.Converters
{
    [ValueConversion(typeof(String), typeof(Model.TagsRow))]
    public class StringToTagConverter : IValueConverter
    {
        Model ActiveModel = (App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel).ActiveModel;
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Model.TagsRow Tag = value as Model.TagsRow;
            if (Tag == null)
            {
                return "";
            }
            String TagName = Tag.Name == null || Tag.Name == String.Empty ? Tag.Address : Tag.Name;

            return "[" + Tag.ControllersRow.Name + "]" + TagName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == "")
            {
                return null;
            }
            String input = (String)value;
            Match ControllerMatch = Regex.Match(input, @"\[(.*?)\]");
            if (!ControllerMatch.Success)
            {
                throw new Exception("Invalid tag string");
            }

            String ControllerName = ControllerMatch.Value.Substring(1, ControllerMatch.Value.Length - 2);
            var MatchingControllers = ActiveModel.Controllers.Where(p => p.Name == ControllerName);
            if (MatchingControllers.Count() == 0)
            {
                throw new Exception("Invalid controller name");
            }
            Model.ControllersRow Controller = MatchingControllers.First();

            String TagName = input.Substring(input.LastIndexOf(']')+1);
            var MatchingTags = ActiveModel.Tags.Where(p => p.Controller == Controller.PK && (p.Address == TagName || p.Name == TagName));
            if (MatchingTags.Count() == 0)
            {
                throw new Exception("Invalid tag address");
            }

            return MatchingTags.First();
        }


    }
}
