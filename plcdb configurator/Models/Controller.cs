using System;
using System.Windows.Media;
using plcdb.Helpers;

namespace plcdb.Models
{
    public class Controller : NotificationObject
    {
        #region Ctor
        public Controller(string name)
        {
            Name = name;
            
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

        #region Type

        private ControllerType _type;
        public ControllerType Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    RaisePropertyChanged(() => Type);
                }
            }
        }

        #endregion

        public enum ControllerType
        {
            opc = 1,
            siemens_s7 = 2,
            ethernet_ip = 3,
            omron = 4
        }

       
    }
}
