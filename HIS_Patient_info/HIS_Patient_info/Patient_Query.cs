using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;

namespace HIS_Patient_info
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Patient_Query" in both code and config file together.
    public class Patient_data_Query : IPatient_Query
    {
        public static List<Tuple<string, string, string, string, string>>  P_Data_passer;

        private async Task QueryPateient(int page, string dbconnection)
        {
            using (TransactionScope Current_Transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (SqlConnection connection = new SqlConnection(dbconnection))
                {
                    var Patient_Query = "select Top(@Page_number *10) Patient_Id,Patient_Name, Patient_Gender,Patient_Address,Patient_Phone from Patient as P join Patient_Visit as V on P.Patient_Id = V.Patient_ID_FK order by V.Patient_Visiting_Date desc";
                    SqlCommand Patient_info = new SqlCommand(Patient_Query, connection);
                    Patient_info.Parameters.AddWithValue("@Page_number", await Task.FromResult(page));
                    connection.Open();
                    await Patient_info.ExecuteNonQueryAsync();
                    SqlDataAdapter Patientreader = new SqlDataAdapter(Patient_info);
                    Patient_info.Dispose();
                    DataTable Patienttable = new DataTable();
                    Patientreader.Fill(Patienttable);
                    ///Tuple里第一个数据类型是病人身份证
                    ///第二个是病人名
                    ///第三个是病人性别
                    ///第四个是病人地址
                    ///第五个是病人电话
                    List<Tuple<string, string, string, string, string>> data = new List<Tuple<string, string, string, string, string>>();
                    var row_number = 0;

                    foreach (DataRow Patient_row in Patienttable.Rows)
                    {
                        row_number += 1;
                        if (row_number >= 1 + (page - 1) * 10 && row_number < 1 + page * 10)
                        {
                            data.Add(Tuple.Create(
                            Patient_row[0].ToString(),
                            Patient_row[1].ToString(),
                            Patient_row[2].ToString(),
                            Patient_row[3].ToString(),
                            Patient_row[4].ToString()));
                        }
                        else if (row_number > page * 10) { break; }
                    }
                    Patienttable.Dispose();
                    connection.Close();
                    P_Data_passer = await Task.FromResult(data);
                }

                Current_Transaction.Complete();
            }
        }
        public void Patient_Data_Acquister(string connection_string, int Query_page_number)
        {
            var Query_task = Task.Run(async () => await QueryPateient(Query_page_number, connection_string));
            Query_task.Wait();
        }
    }
}
