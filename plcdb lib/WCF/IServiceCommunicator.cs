using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace plcdb_lib.WCF
{

    [ServiceContract]
    public interface IServiceCommunicator
    {
        [OperationContract]
        void SetActiveModelPath(String ActiveModelPath);

    }
}