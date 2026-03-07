using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ApartmentChain.Pages
{
    public partial class ProfilePage : Page
    {
        private Users _currentUser;
        public ProfilePage(Users user)
        {
            InitializeComponent();

            if (user != null)
                _currentUser = user;
            else
                _currentUser = new Users();

            DataContext = _currentUser;

            SetEditMode(false);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login)) errors.AppendLine("Поле для логина не может быть пустым");
            //if (string.IsNullOrWhiteSpace(_currentUser.FullName)) errors.AppendLine("Поле для ФИО не может быть пустым");
            //if (!string.IsNullOrWhiteSpace(_currentUser.Email) && !Regex.IsMatch(_currentUser.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.AppendLine("Неверный формат электронной почты, пример: \"empty@business.com\"");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Изменения успешно сохранены!", "Ура!", MessageBoxButton.OK, MessageBoxImage.Information);
                SetEditMode(false);
                if (NavigationService != null)
                    NavigationService.Navigate(new MainPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Exit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Session.IsAuthorized = false;
            Session.CurrentUserLogin = null;

            if (NavigationService != null) NavigationService.Navigate(new MainPage());
        }

        private void SetEditMode(bool isEditable)
        {
            //LoginTextBox.IsReadOnly = !isEditable;
            //FIOTextBox.IsReadOnly = !isEditable;
            //EmailTextBox.IsReadOnly = !isEditable;
            //PhoneNumberTextBox.IsReadOnly = !isEditable;

            //SaveButton.Visibility = isEditable ? Visibility.Visible : Visibility.Collapsed;
            //EditButton.Visibility = isEditable ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
