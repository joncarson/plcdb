using System;
using System.Windows.Media;
using plcdb.Helpers;

namespace plcdb.Models
{
    public class Query : NotificationObject
    {
        #region Ctor
        public Query(string name, int age, bool isMarried, double height, DateTime birthDate, Color favColor)
        {
            
        }
        #endregion

        #region Name

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        #endregion

    }
}
