using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace plcdb_service.Licensing
{
    class ServiceLicenseProvider : LicenseProvider
    {
        public override License GetLicense(LicenseContext context, Type type, object inst, bool allowExceptions)
        {
            if (type != typeof(plcdb) && allowExceptions)
                throw new LicenseException(type);

            try
            {
                plcdb instance = (plcdb)inst;
                return instance.License;
            }
            catch (Exception e)
            {
                if (!allowExceptions)
                    return ServiceLicense.CreateDemoLicense();
                throw e;
            }
        }

    }
}
