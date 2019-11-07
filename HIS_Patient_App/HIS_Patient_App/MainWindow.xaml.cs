using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using HIS_Patient_info;

namespace HIS_Patient_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<string> Data_buffer_DML = new List<string>(4);
        public static string Data_buffer_condition_param;
        public int Page_number = 1;
        private const string dbconnection = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HIS_Data;Integrated Security=True;Pooling=False";
        private bool No_next_page;


        /// <summary>
        /// 病人查询 增删改，调用His_patient_info中间件
        /// 时间使用国家授时时间
        /// 数据查询使用中间件所生成的序列化数据，再由WPF客户端获取数据
        /// 数据增删改部分由客户端生成序列化数据，再调用中间件获取
        /// </summary>
        /// <param name="O_interface_obj"></param>
        
        //数据查询中间件调用
        private void M_access_interface_query_service(IPatient_Query O_interface_obj)
        {
            O_interface_obj.Patient_Data_Acquister(dbconnection, Page_number);
        }

        //数据增删改中间件调用
        private void M_access_interface_DML_service(IPatient_Service O_interface_obj)
        {
            HIS_data_Sender T_data_transfer = new HIS_data_Sender();
            O_interface_obj.Patient_Data_Handler(dbconnection, T_data_transfer.Data_sender, T_data_transfer.Condition_sender);
        }

        //获取中间数据查询结果
        private async void List_Patient_info()
        {
            //生成以类为单位的list
            List<HIS_Data_Set> Binding_Data = new List<HIS_Data_Set>(); 
            //生成服务端的实例
            Patient_data_Query Query = new Patient_data_Query();
            //调用服务
            M_access_interface_query_service(Query);

            //获取服务器端序列化类的数据
            HIS_Composit T_serialized_patient_data = await Task.FromResult(new HIS_Composit());

            //将数据parse并binding到wpf中
            if (T_serialized_patient_data.Patient_Info_List.Count==0) { No_next_page = true; }
            else
            {
                for (int i = 0; i < T_serialized_patient_data.Patient_Info_List.Count; i++)
                {
                    HIS_Data_Set Data_instance = new HIS_Data_Set();

                    Data_instance.PatientID = T_serialized_patient_data.Patient_Info_List[i].Item1;
                    Data_instance.PatientName = T_serialized_patient_data.Patient_Info_List[i].Item2;
                    Data_instance.PatientGender = T_serialized_patient_data.Patient_Info_List[i].Item3;
                    Data_instance.PatientAddress = T_serialized_patient_data.Patient_Info_List[i].Item4;
                    Data_instance.PatientPhone = T_serialized_patient_data.Patient_Info_List[i].Item5;
                    Binding_Data.Add(Data_instance);
                }
                HIS_TABLE.ItemsSource = Binding_Data;
                No_next_page = false;
            }

        }

        //信息修改初始化方法
        private void m_Data_modifier()
        {
            Data_buffer_DML.Clear();
            Data_buffer_DML.Add(ID_Card.Text);
            Data_buffer_DML.Add(Patient_name.Text);
            Data_buffer_DML.Add(Patient_gender.Text);
            Data_buffer_DML.Add(Patient_address.Text);
            Data_buffer_DML.Add(Patient_phone.Text);

            Patient_data_Modify Modification = new Patient_data_Modify();
            M_access_interface_DML_service(Modification);
        }

        //删除数据调用方法
        private void Data_cleaner()
        {
            var Patient_info = (HIS_Data_Set)HIS_TABLE.SelectedItem;
            Data_buffer_condition_param = Patient_info.PatientID.ToString();
            Patient_data_Delete Purge = new Patient_data_Delete();
            M_access_interface_DML_service(Purge);
        }

        //添加信息初始化方法
        private void Data_Writer()
        {
            Data_buffer_DML.Clear();
            if (ID_Card.Text != "" | Patient_name.Text != "" | Patient_address.Text != "" |
                Patient_gender.Text != "" | Patient_phone.Text != "")
            {
                Data_buffer_DML.Add(ID_Card.Text);
                Data_buffer_DML.Add(Patient_name.Text);
                Data_buffer_DML.Add(Patient_gender.Text);
                Data_buffer_DML.Add(Patient_address.Text);
                Data_buffer_DML.Add(Patient_phone.Text);

                Patient_data_Add Acquister = new Patient_data_Add();

                M_access_interface_DML_service(Acquister);
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            List_Patient_info();
        }

        //下一页按钮方法
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            Page_number += 1;
            List_Patient_info();
            if(No_next_page == true) { Page_number -= 1; }
            else { }
            Page.Content = Page_number;
        }

        //上一页按钮方法
        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            Page_number -= 1;
            if (Page_number < 1)
            {
                Page_number = 1;
            }
            Page.Content = Page_number;
            List_Patient_info();
        }

        //修改病人信息按钮，执行方法
        private void M_patient_modify_Click(object sender, RoutedEventArgs e)
        {
            var Patient_info = (HIS_Data_Set)HIS_TABLE.SelectedItem;

            if(HIS_TABLE.SelectedItem == null)
            {
                MessageBoxResult No_selection = MessageBox.Show("请选择需要修改信息的病人","提示",MessageBoxButton.OK, MessageBoxImage.Information);
                if(No_selection == MessageBoxResult.OK) { }
            }
            else
            {
                if (P_confirm_modification.Visibility != Visibility.Visible)
                {
                    input_form.Visibility = Visibility.Visible;
                    P_confirm_modification.Visibility = Visibility.Visible;
                    Data_Modify_Cancel.Visibility = Visibility.Visible;
                    P_confirm_adding.Visibility = Visibility.Hidden;
                    Data_Add_Cancel.Visibility = Visibility.Hidden;
                }

                if (Patient_info == null) { }
                else
                {
                    Data_buffer_condition_param = Patient_info.PatientID.ToString();

                    ID_Card.Text = Patient_info.PatientID.ToString();
                    Patient_name.Text = Patient_info.PatientName;
                    Patient_address.Text = Patient_info.PatientAddress;
                    Patient_gender.Text = Patient_info.PatientGender;
                    Patient_phone.Text = Patient_info.PatientPhone.ToString();
                }
            }
            
        }

        //确认修改按钮，执行方法
        private void M_confirm_modification_Click(object sender, RoutedEventArgs e)
        {
            if (input_form.Visibility == Visibility.Visible)
            {
                m_Data_modifier();
                List_Patient_info();
            }
        }

        //添加病人信息按钮，执行方法
        private void M_patient_add_Click(object sender, RoutedEventArgs e)
        {
            //initializing new text inpu text block for id input, name input etc.
            P_confirm_modification.Visibility = Visibility.Hidden;
            Data_Modify_Cancel.Visibility = Visibility.Hidden;
            HIS_TABLE.SelectedItem = null;
            HIS_TABLE.UnselectAll();

            ID_Card.Text = "";
            Patient_name.Text = "";
            Patient_address.Text = "";
            Patient_gender.Text = "";
            Patient_phone.Text = "";

            if (P_confirm_adding.Visibility != Visibility.Visible)
            {
                input_form.Visibility = Visibility.Visible;
                P_confirm_adding.Visibility = Visibility.Visible;
                Data_Add_Cancel.Visibility = Visibility.Visible;  
            }
        }

        //删除病人信息按钮，执行方法
        private void M_patient_delete_Click(object sender, RoutedEventArgs e)
        {
            var Patient_info = (HIS_Data_Set)HIS_TABLE.SelectedItem;
            if (Patient_info != null)
            {
                var Caption_Sentence_Beginning = "确定要删除";
                var Caption_Sentence_end = "的信息么？";
                StringBuilder Delete_remider = new StringBuilder(Patient_info.PatientName.Trim().Length + Caption_Sentence_Beginning.Length + Caption_Sentence_end.Length);
                Delete_remider.Append(Caption_Sentence_Beginning);
                Delete_remider.Append(Patient_info.PatientName.Trim());
                Delete_remider.Append(Caption_Sentence_end);
                MessageBoxResult Confirm_delete = MessageBox.Show(Delete_remider.ToString(), "删除病人资料", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (Confirm_delete == MessageBoxResult.Cancel) { }
                else
                {
                    Data_cleaner();
                    List_Patient_info();

                    input_form.Visibility = Visibility.Hidden;
                    P_confirm_adding.Visibility = Visibility.Hidden;
                    Data_Add_Cancel.Visibility = Visibility.Hidden;
                    P_confirm_modification.Visibility = Visibility.Hidden;
                    Data_Modify_Cancel.Visibility = Visibility.Hidden;

                    HIS_TABLE.SelectedItem = null;
                    HIS_TABLE.UnselectAll();
                    ID_Card.Text = "";
                    Patient_name.Text = "";
                    Patient_address.Text = "";
                    Patient_gender.Text = "";
                    Patient_phone.Text = "";
                }
            }
            else { MessageBox.Show("请先选择病人", "提示", MessageBoxButton.OK, MessageBoxImage.Information); }
        }

        //确认添加按钮，执行方法
        private void M_confirm_adding_Click(object sender, RoutedEventArgs e)
        {
            if (input_form.Visibility == Visibility.Visible)
            {
                Data_Writer();
                List_Patient_info();

                ID_Card.Text = "";
                Patient_name.Text = "";
                Patient_address.Text = "";
                Patient_gender.Text = "";
                Patient_phone.Text = "";
            }
            else { }
        }

        //取消修改按钮，执行方法
        private void Cancel_Modify(object sender, RoutedEventArgs e)
        {
            input_form.Visibility = Visibility.Hidden;
            P_confirm_modification.Visibility = Visibility.Hidden;
            Data_Modify_Cancel.Visibility = Visibility.Hidden;
            HIS_TABLE.SelectedItem = null;
            HIS_TABLE.UnselectAll();
            ID_Card.Text = "";
            Patient_name.Text = "";
            Patient_address.Text = "";
            Patient_gender.Text = "";
            Patient_phone.Text = "";
        }

        //取消添加按钮，执行方法
        private void Cacel_Add(object sender, RoutedEventArgs e)
        {
            input_form.Visibility = Visibility.Hidden;
            P_confirm_adding.Visibility = Visibility.Hidden;
            Data_Add_Cancel.Visibility = Visibility.Hidden;
            ID_Card.Text = "";
            Patient_name.Text = "";
            Patient_address.Text = "";
            Patient_gender.Text = "";
            Patient_phone.Text = "";
        }
    }

    //数据发送
    [Serializable]
    public class HIS_data_Sender
    {
        public List<string> Data_sender = MainWindow.Data_buffer_DML;
        public string Condition_sender = MainWindow.Data_buffer_condition_param;
    }

    //数据接收
    public class HIS_Data_Set
    {
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string PatientGender { get; set; }
        public string PatientAddress { get; set; }
        public string PatientPhone { get; set; }
    }
}
