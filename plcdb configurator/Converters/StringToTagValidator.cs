using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using plcdb_lib.Models;
using plcdb.ViewModels;

namespace plcdb.Converters
{
    class StringToTagValidator : ValidationRule
    {
        Model ActiveModel = (App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel).ActiveModel;
        public StringToTagValidator()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo ultureInfo)
        {
            if (value == null || value == "")
            {
                return null;
            }
            String input = (String)value;
            Match ControllerMatch = Regex.Match(input, @"\[(.*?)\]");
            if (!ControllerMatch.Success)
            {
                return new ValidationResult(false, "Invalid controller name");
            }

            String ControllerName = ControllerMatch.Value.Substring(1, ControllerMatch.Value.Length - 2);
            var MatchingControllers = ActiveModel.Controllers.Where(p => p.Name == ControllerName);
            if (MatchingControllers.Count() == 0)
            {
                return new ValidationResult(false, "Invalid controller name");
            }
            Model.ControllersRow Controller = MatchingControllers.First();

            String TagName = input.Substring(input.LastIndexOf(']')+1);
            var MatchingTags = ActiveModel.Tags.Where(p => p.Controller == Controller.PK && (p.Address == TagName || p.Name == TagName));
            if (MatchingTags.Count() == 0)
            {
                return new ValidationResult(false, "Invalid tag address");
            }

            return new ValidationResult(true, null);
        }
        
    }
}
