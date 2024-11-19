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
using static System.Collections.Specialized.BitVector32;

namespace Praca_inzynierska
{
    /// <summary>
    /// Logika interakcji dla klasy Register_Data.xaml
    /// </summary>
    public partial class Register_Data : Window
    {

        private SqlConnection sqlConnection = null;
        public Register_Data()
        {
            InitializeComponent();
        }

       
        private void DateOfBirthPicker_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            if (DateOfBirthPicker.SelectedDate.HasValue)
            {
                DateTime birthDate = DateOfBirthPicker.SelectedDate.Value;
                var (years, days) = CalculateAge(birthDate);

                // Sprawdzanie, czy wiek wynosi dokładnie 1 dzień
                if (years == 0 && days == 1)
                {
                    MessageBox.Show("Masz dokładnie 1 dzień!", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Sprawdzanie, czy wiek jest większy niż 100 lat
                if (years > 100 || years < 0)
                {
                    MessageBox.Show("Wiek nie może być większy niż 100 lat lub ujemny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DateOfBirthPicker.SelectedDate = null; // Opcjonalnie resetujemy datę urodzenia
                }
                else if (years >= 0)
                {
                    // Wyświetlanie pełnych lat i dni
                    AgeTextBlock.Text = $"{years} lat {days} dni";
                }
            }
        }

        private (int years, int days) CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Now;
            int ageYears = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-ageYears))
            {
                ageYears--;
            }

            // Obliczanie dni, które minęły po ostatnich urodzinach
            DateTime lastBirthday = birthDate.AddYears(ageYears);
            int days = (today - lastBirthday).Days;

            return (ageYears, days);
        }

        private void Save_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Pobranie danych z kontrolek
            string firstName = FirstName.Text;
            string lastName = LastName.Text; 
            string pessel = Pessel.Text;
            string phoneNumber = Phone.Text;
            string email = Mail.Text;
            string bloodType = (DoctorComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            DateTime? birthDate = DateOfBirthPicker.SelectedDate;

            int.TryParse(phoneNumber, out int phone);
            if(phone == 0)
            {
                MessageBox.Show("Błąd: Nie poprawny numer telefonu");
                return;
            }



            // Pobranie ID zalogowanego użytkownika z sesji
            int loggedInUserId = Session.LoggedInUserId;

            if (loggedInUserId <= 0)
            {
                MessageBox.Show("Błąd: Użytkownik nie jest zalogowany.");
                return;
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || birthDate == null)
            {
                MessageBox.Show("Proszę wypełnić wszystkie wymagane pola.");
                return;
            }

            // Łączenie z bazą danych
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PracaDb"].ConnectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string query = @"
                INSERT INTO ClientData (UserId, FirstName, LastName, Pessel, NumberPhone, E_Mail, BloodType, BirthDate)
                VALUES (@UserId, @FirstName, @LastName, @Pessel, @NumberPhone, @E_Mail, @BloodType, @BirthDate)";

                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@UserId", loggedInUserId);
                        sqlCommand.Parameters.AddWithValue("@FirstName", firstName);
                        sqlCommand.Parameters.AddWithValue("@LastName", lastName);
                        sqlCommand.Parameters.AddWithValue("@Pessel", string.IsNullOrEmpty(pessel) ? (object)DBNull.Value : pessel);
                        sqlCommand.Parameters.AddWithValue("@NumberPhone", string.IsNullOrEmpty(phoneNumber) ? (object)DBNull.Value : phoneNumber);
                        sqlCommand.Parameters.AddWithValue("@E_Mail", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                        sqlCommand.Parameters.AddWithValue("@BloodType", string.IsNullOrEmpty(bloodType) ? (object)DBNull.Value : bloodType);
                        sqlCommand.Parameters.AddWithValue("@BirthDate", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value);

                        sqlCommand.ExecuteNonQuery();
                    }

                    if (!string.IsNullOrEmpty(pessel) && (!pessel.All(char.IsDigit) || pessel.Length != 11))
                    {
                        MessageBox.Show("PESEL musi zawierać dokładnie 11 cyfr.");
                        return;
                    }

                    MessageBox.Show("Dane zostały zapisane pomyślnie.");
                    var login = new Login();
                    this.Close();
                    login.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas zapisu danych: {ex.Message}");
                }
            }
        }
        public static class Session
        {
            public static int LoggedInUserId { get; set; } = 0;
        }

        private void Pessel_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

    }


}
