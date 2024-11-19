using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using System.Windows.Shapes;
using static Praca_inzynierska.Register_Data;

namespace Praca_inzynierska
{
    /// <summary>
    /// Logika interakcji dla klasy Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        private SqlConnection sqlConnection = null;
        public Register()
        {
            InitializeComponent();
        }

        private void Register_Btn_Click(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PracaDb"].ConnectionString);
            sqlConnection.Open();

            // Dodanie nowego użytkownika do tabeli
            var addData = "INSERT INTO [dbo].[Users] (Username, Password) OUTPUT INSERTED.Id VALUES(@username, @password)";
            var cmd = new SqlCommand(addData, sqlConnection);

            var hashPassword = md5_sql_hash.hashPassword(password.Password);

            cmd.Parameters.AddWithValue("@username", username.Text);
            cmd.Parameters.AddWithValue("@password", hashPassword);

            int userId = (int)cmd.ExecuteScalar();
            sqlConnection.Close();

            Session.LoggedInUserId = userId;

            username.Text = string.Empty;
            password.Password = string.Empty;

            var registerData = new Register_Data();
            this.Close();
            registerData.Show();
        }
    }
}
