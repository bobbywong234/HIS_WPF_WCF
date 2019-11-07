using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Transactions;
using System.Security.Authentication;
using System.Net.Security;



namespace HIS_Patient_info
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“Patient_Service”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 Patient_Service.svc 或 Patient_Service.svc.cs，然后开始调试。
    public class Patient_data_Delete : IPatient_Service
    {
        public static string Delete_action_status;
        private async Task DeletePatient(string dbconnection, string Identity)
        {
            try{
                using (TransactionScope Current_Transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (SqlConnection connection = new SqlConnection(dbconnection))
                    {
                        var Delete_Patient = "Delete from Patient where Patient_Id = @ID";
                        SqlCommand Data_remove = new SqlCommand(Delete_Patient, connection);
                        Data_remove.Parameters.AddWithValue("@ID", await Task.FromResult(Identity));
                        connection.Open();
                        await Data_remove.ExecuteNonQueryAsync();
                        Data_remove.Dispose();
                        connection.Close();
                    }
                    Current_Transaction.Complete();
                    Delete_action_status = "病人信息删除成功";
                }
            }
            catch(TransactionAbortedException)
            {
                using (Transaction rollback = Transaction.Current)
                {
                    rollback.Rollback();
                    Delete_action_status = "删除操作已退回";
                }
            }
        }
        public void Patient_Data_Handler(string connection_string, List<string> WPF_Two_way_Binding_data = null, string Database_indexer = null)
        {
            var Delete_task = Task.Run(async () => await DeletePatient(connection_string, Database_indexer));
            Delete_task.Wait();
        }
    }

    public class Patient_data_Add : IPatient_Service
    {
        public static string Insert_action_Status; 
        private async Task AddPatient(string dbconnection, List<string> Patient_info)
        {
            try
            {
                using (TransactionScope Current_Transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (SqlConnection connection = new SqlConnection(dbconnection))
                    {
                        var Add_Patient = "insert into Patient(Patient_Id, Patient_Name, Patient_Gender, Patient_Address, Patient_Phone) values(@ID,@Name,@Gender,@Address,@Phone)";
                        SqlCommand Data_add = new SqlCommand(Add_Patient, connection);
                        Data_add.Parameters.AddWithValue("@ID", await Task.FromResult(Patient_info[0]));
                        Data_add.Parameters.AddWithValue("@Name", await Task.FromResult(Patient_info[1]));
                        if (Patient_info.Count < 3) { }
                        else
                        {
                            Data_add.Parameters.AddWithValue("@Gender", await Task.FromResult(Patient_info[2]));
                            Data_add.Parameters.AddWithValue("@Address", await Task.FromResult(Patient_info[3]));
                            Data_add.Parameters.AddWithValue("@Phone", await Task.FromResult(Patient_info[4]));
                        }
                        connection.Open();
                        await Data_add.ExecuteNonQueryAsync();
                        Data_add.Dispose();
                        connection.Close();
                    }
                    Current_Transaction.Complete();
                    Insert_action_Status = "添加病人信息成功";
                }
            }
            catch(TransactionAbortedException)
            {
                using (Transaction rollback = Transaction.Current)
                {
                    rollback.Rollback();
                    Insert_action_Status = "信息添加操作已取消";
                }
            }
        }

        private async Task AddPatient_FK(string dbconnection, List<string> Patient_info,DateTime Atomic_Date)
        {
            try
            {
                using (TransactionScope Current_Transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (SqlConnection connection = new SqlConnection(dbconnection))
                    {
                        if (Atomic_Date != null)
                        {
                            var Add_Patient_FK_table = "insert into Patient_Visit(Patient_ID_FK,Patient_Visiting_Date) values (@FK_ID,@Visiting_date)";
                            SqlCommand FK_data_add = new SqlCommand(Add_Patient_FK_table, connection);
                            FK_data_add.Parameters.AddWithValue("@FK_ID", await Task.FromResult(Patient_info[0]));
                            FK_data_add.Parameters.AddWithValue("@Visiting_date", Atomic_Date);
                            connection.Open();
                            await FK_data_add.ExecuteNonQueryAsync();
                            FK_data_add.Dispose();
                        }
                    }

                    Current_Transaction.Complete();
                }
            }
            catch(TransactionAbortedException)
            {
                using (Transaction rollback = Transaction.Current)
                {
                    rollback.Rollback();
                }
            }
        }

        private async Task<DateTime> GetAtomic_Time()
        {
            var DNS_resolver = await Dns.GetHostEntryAsync("ntp.ntsc.ac.cn");
            var End_point = new IPEndPoint(DNS_resolver.AddressList[0], 123);
            var ntp_data_buffer = new byte[48]; //buffer长度
            ntp_data_buffer[0] = 0x1B;

            using (var Access_datetime_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                Access_datetime_socket.Connect(End_point);
                Access_datetime_socket.ReceiveTimeout = 5000;
                Access_datetime_socket.SendTimeout = 1000;
                Access_datetime_socket.Send(ntp_data_buffer);

                Access_datetime_socket.Receive(ntp_data_buffer);
                Access_datetime_socket.Close();
            }

            ulong intPart = (ulong)ntp_data_buffer[40] << 24 | (ulong)ntp_data_buffer[41] << 16 | (ulong)ntp_data_buffer[42] << 8 | (ulong)ntp_data_buffer[43];
            ulong fractPart = (ulong)ntp_data_buffer[44] << 24 | (ulong)ntp_data_buffer[45] << 16 | (ulong)ntp_data_buffer[46] << 8 | (ulong)ntp_data_buffer[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var Atomic_Time = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds + 8*60*60*1000);
            return Atomic_Time;
        }

        public void Patient_Data_Handler(string connection_string, List<string> WPF_Two_way_Binding_data = null, string Database_indexer = null)
        {
            var AddData_task = Task.Run(async () => await AddPatient(connection_string, WPF_Two_way_Binding_data));
            AddData_task.Wait();
            var Get_precise_time = Task.Run(async () => await GetAtomic_Time());
            Get_precise_time.Wait();
            if (Get_precise_time.IsCompleted == true && Insert_action_Status == "添加病人信息成功")
            {
                var AddFKData_task = Task.Run(async () => await AddPatient_FK(connection_string, WPF_Two_way_Binding_data, Get_precise_time.Result));
                AddFKData_task.Wait();
            }
        }
    }

    public class Patient_data_Modify : IPatient_Service
    {
        public static string Updata_Action_Status;
        private async Task ModifyPatient(string dbconnection, List<string> Patient_info, string Current_id)
        {
            try 
            {
                using (TransactionScope Current_Transcation = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (SqlConnection connection = new SqlConnection(dbconnection))
                    {
                        var Modify_Patient = "update Patient set Patient_Id = @ID, Patient_Name = @Name, Patient_Gender = @Gender, Patient_Address = @Address, Patient_Phone= @Phone where Patient_Id =@Current_id";
                        SqlCommand Modify = new SqlCommand(Modify_Patient, connection);
                        Modify.Parameters.AddWithValue("@ID", await Task.FromResult(Patient_info[0]));
                        Modify.Parameters.AddWithValue("@Name", await Task.FromResult(Patient_info[1]));
                        Modify.Parameters.AddWithValue("@Gender", await Task.FromResult(Patient_info[2]));
                        Modify.Parameters.AddWithValue("@Address", await Task.FromResult(Patient_info[3]));
                        Modify.Parameters.AddWithValue("@Phone", await Task.FromResult(Patient_info[4]));
                        Modify.Parameters.AddWithValue("@Current_id", await Task.FromResult(Current_id));
                        connection.Open();
                        await Modify.ExecuteNonQueryAsync();
                        Modify.Dispose();
                        connection.Close();
                    }

                    Current_Transcation.Complete();
                    Updata_Action_Status = "病人更新信息已";
                }
            }
            catch(TransactionAbortedException) 
            {
                using (Transaction rollback = Transaction.Current)
                {
                    rollback.Rollback();
                    Updata_Action_Status = "信息更新操作已取消";
                }
            }
            
        }

        public void Patient_Data_Handler(string connection_string, List<string> WPF_Two_way_Binding_data = null, string Database_indexer = null)
        {
            var Data_Updata_task = Task.Run(async () => await ModifyPatient(connection_string, WPF_Two_way_Binding_data, Database_indexer));
            Data_Updata_task.Wait();
        }
    }
}

