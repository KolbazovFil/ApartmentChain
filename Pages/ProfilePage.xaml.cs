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
        }

        private void Exit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Session.IsAuthorized = false;
            Session.CurrentUserLogin = null;
            (Application.Current.MainWindow as MainWindow)?.UpdateUI();

            if (NavigationService != null) NavigationService.Navigate(new MainPage());
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditBookingButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            string currentUserLogin = Session.CurrentUserLogin;
            if (!string.IsNullOrEmpty(currentUserLogin))
            {
                var result = MessageBox.Show("Вы действительно хотите удалить свой профиль?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var context = Entities.GetContext();
                        var user = context.Users.FirstOrDefault(u => u.Login == currentUserLogin);

                        if (user != null)
                        {
                            context.Users.Remove(user);
                            context.SaveChanges();
                            MessageBox.Show("Профиль успешно удалён.", "Удаление профиля", MessageBoxButton.OK, MessageBoxImage.Information);
                            Session.IsAuthorized = false;
                            Session.CurrentUserLogin = null;
                            (Application.Current.MainWindow as MainWindow)?.UpdateUI();
                            if (NavigationService != null) NavigationService.Navigate(new MainPage());
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
        }
    }
}