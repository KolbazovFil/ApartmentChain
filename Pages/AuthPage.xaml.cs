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

                if (password != user.PasswordHash)
                {
                    MessageBox.Show("Неверный пароль!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                Session.IsAuthorized = true;
                Session.CurrentUserLogin = user.Login;
                ((MainWindow)Application.Current.MainWindow).UpdateUI();

                MessageBox.Show("Добро пожаловать, " + user.Surename + " " + user.Name, "Успех");

                NavigationService.Navigate(new MainPage());
            }
            return true;
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