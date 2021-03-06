﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace plcdb_lib.Models.Controllers
{
    public abstract class ControllerBase
    {
        public abstract object Read(Model.TagsRow t);
        public abstract bool   Write(Model.TagsRow t, object val);
        public abstract bool ValidateTag(String address);
        public abstract Model.TagsDataTable BrowseTags();
        

        public ControllerBase(Model.ControllersRow args)
        {
        }

        public ControllerBase()
        {
        }
    }

    public class AddressNotFoundException : Exception
    {
        public AddressNotFoundException() :  base("Read failed. Address not found in PLC.")
        {
            
        }

        public AddressNotFoundException(string message)
            : base(message)
        {
        }

        public AddressNotFoundException(string message,
          Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
