using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace HIS_Patient_info
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IPatient_Service”。
    [ServiceContract]
    public interface IPatient_Service
    {
        [OperationContract]
        void Patient_Data_Handler
        (
          string connection_string, 
          List<string> WPF_Two_way_Binding_data = null,
          string Database_indexer = null
        );
    }

}
