﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Data.Sql;
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

namespace Praca_inzynierska
{
    /// <summary>
    /// Logika interakcji dla klasy Login.xaml
    /// </summary>
    public partial class Login : Window
    {

        private SqlConnection sqlConnection = null;
        public Login()
        {
            InitializeComponent();
        }

        private void LoginDoctor_Btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Register_Btn_Click_Two(object sender, RoutedEventArgs e)
        {
            var register = new Register();
            this.Close();
            register.Show();
        }

        private void Login_Btn_Click(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PracaDb"].ConnectionString);
            sqlConnection.Open();

            var add_data = "SELECT * FROM [dbo].[Users] WHERE username=@username AND password=@password";
            var cmd = new SqlCommand(add_data, sqlConnection);

            var hashPassword = md5_sql_hash.hashPassword(password.Password);

            cmd.Parameters.AddWithValue("@username", username.Text);
            cmd.Parameters.AddWithValue("@password", hashPassword);

            cmd.ExecuteNonQuery();
            int Count = Convert.ToInt32(cmd.ExecuteScalar());

            sqlConnection.Close();
            username.Text = string.Empty;
            password.Password = string.Empty;
            if (Count > 0)
            {
                var mainClient = new MainClient();
                this.Close();
                mainClient.Show();
            }
            else
            {
                MessageBox.Show("Password or Username is not correct");
            }
        }
    }
}
