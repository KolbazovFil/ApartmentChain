using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using static ApartmentChain.MainWindow;

namespace ApartmentChain.Pages
{
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            if (Auth(LoginTextBox.Text, PasswordBox.Password)) Clear();
        }

        public bool Auth(string login, string password)
        {
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(login) || (string.IsNullOrWhiteSpace(password)))
                errors.AppendLine("Поля логина и пароля должны быть заполненны!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            using (var db = new Entities())
            {
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Login == login);
                if (user == null)
                {
                    MessageBox.Show("Пользователь с таким логином не найден!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    MessageBox.Show("Неверный пароль!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                Session.IsAuthorized = true;
                Session.CurrentUserLogin = user.Login;
                ((MainWindow)Application.Current.MainWindow).UpdateUI();

                MessageBox.Show("Пользователь успешно найден!", "Успех");

                if (NavigationService != null)
                {
                    if (user.RoleID == 1)
                    {
                        NavigationService.Navigate(new MainPageAdmin());
                    }
                    else
                    {
                        NavigationService.Navigate(new MainPage());
                    }
                }
                return true;
            }
        }

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            byte[] hashBytes;

            try
            {
                hashBytes = Convert.FromBase64String(storedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            byte[] storedHashBytes = new byte[20];
            Array.Copy(hashBytes, 16, storedHashBytes, 0, 20);

            using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                for (int i = 0; i < 20; i++)
                {
                    if (hash[i] != storedHashBytes[i])
                        return false;
                }
                return true;
            }
        }

        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null) NavigationService.Navigate(new RegPage(null));
            Clear();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            Clear();
        }

        private void Clear()
        {
            LoginTextBox.Clear();
            PasswordBox.Clear();
        }
    }
}
