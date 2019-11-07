using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace HIS_Patient_info
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPatient_Query" in both code and config file together.
    [ServiceContract]
    public interface IPatient_Query
    {
        [OperationContract]
        void Patient_Data_Acquister(string connection_string, int Query_page_number);
    }

    [DataContract]
    [Serializable]
    public class HIS_Composit
    {
        [DataMember(EmitDefaultValue = false)]
        public List<Tuple<string, string, string, string, string>> Patient_Info_List = Patient_data_Query.P_Data_passer;
    }

}
