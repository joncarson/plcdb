using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace plcdb_lib.WCF
{

    [ServiceContract ]
    public interface IServiceCommunicator
    {
        [OperationContract]
        void SetActiveModelPath(String ActiveModelPath);

        [OperationContract]
        List<ObjectStatus> GetQueriesStatus();

        [OperationContract]
        List<WcfEvent> GetLatestLogs(DateTime MinDate);

        [OperationContract]
        DateTime GetStartTime();

        [OperationContract]
        String GetUniqueHWID();

        [OperationContract]
        void SetLicense(String PurchaseKey, String LicenseKey);

        
    }

    [DataContract(Namespace="plcdb_lib", Name="ObjectStatus")]
    public class ObjectStatus
    {
        [DataMember]
        public long PK {get;set;}

        [DataMember]
        public StatusEnum Status {get;set;}
    }

    [DataContract(Namespace="plcdb_lib", Name="StatusEnum")]
    public enum StatusEnum
    {
        [EnumMember]
        Good,
        [EnumMember]
        NotRunning,
        [EnumMember]
        Error
    }
    [DataContract(Namespace="plcdb_lib", Name="WcfEvent")]
    public class WcfEvent
    {
        [DataMember]
        public String Message { get; set; }

        [DataMember]
        public String LogLevel { get; set; }

        [DataMember]
        public String StackTrace { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public int Query { get; set; }

        [DataMember]
        public DateTime Occurred { get; set; }
    }
}