using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ApartmentChain.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }
        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            Registration(NameTextBox.Text, SurnameTextBox.Text, LoginTextBox.Text, PasswordBox.Password, ConfirmPasswordBox.Password, BirthdayDatePicker.SelectedDate?.ToString("dd-MM-yyyy"), PhoneNumberTextBox.Text);
        }
        public bool Registration(string name, string surname, string login, string password, string confirmPassword, string birthday, string phone)
        {
            string hashedPassword = HashPassword(password);
            DateTime? birthdayDate = null;
            if (!string.IsNullOrWhiteSpace(birthday))
            {
                if (DateTime.TryParseExact(birthday, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    birthdayDate = parsedDate;
                    DateTime today = DateTime.Today;
                    DateTime eighteenYearsAgo = today.AddYears(-18);
                    if (birthdayDate > eighteenYearsAgo)
                    {
                        MessageBox.Show("Вам должно быть больше 18 лет", "Возрастное ограничение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Некорректный формат даты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(login) 
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(confirmPassword) 
                || string.IsNullOrWhiteSpace(name) 
                || string.IsNullOrWhiteSpace(surname)
                || string.IsNullOrWhiteSpace(birthday)
                || string.IsNullOrWhiteSpace(phone))
                errors.AppendLine("Все поля должны быть заполнены");

            if (login.Contains(" ")) errors.AppendLine("Поле для логина не должно содержать пробелы");

            if (confirmPassword != password) errors.AppendLine("Пароли не совпадают");

            if (!Regex.IsMatch(password, @"^(?=.*[0-9])(?=.*[a-zA-Z]).{6,}$"))
                errors.AppendLine("Пароль должен содержать минимум 6 символов, хотя бы одну цифру и только латинскую раскладку");

            if (!string.IsNullOrWhiteSpace(phone))
            {
                var cleanedPhone = Regex.Replace(phone, @"\D", "");

                if (cleanedPhone.StartsWith("8"))
                {
                    cleanedPhone = "7" + cleanedPhone.Substring(1);
                }

                if (!Regex.IsMatch(cleanedPhone, @"^7\d{10}$"))
                {
                    errors.AppendLine("Неверный формат номера телефона, пример: \"+71234567890\"");
                }
                else
                {
                    phone = "+" + cleanedPhone;
                }
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using (var db = new Entities())
            {
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Login == login);

                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return false;
                }

                Users userObject = new Users
                {
                    Name = name,
                    Surename = surname,
                    Login = login,
                    PasswordHash = hashedPassword,
                    Birthday = birthdayDate ?? DateTime.MinValue,
                    PhoneNumber = phone,
                    RoleID = 2
                };

                db.Users.Add(userObject);
                db.SaveChanges();
                MessageBox.Show("Регистрация прошла успешно!");
                if (NavigationService != null) NavigationService.Navigate(new AuthPage());
                Clear();
                return true;
            }
        }
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);

                return Convert.ToBase64String(hashBytes);
            }
        }
        private void Clear()
        {
            NameTextBox.Clear();
            SurnameTextBox.Clear();
            LoginTextBox.Clear();
            PasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            PhoneNumberTextBox.Clear();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
            Clear();
        }
    }
}
