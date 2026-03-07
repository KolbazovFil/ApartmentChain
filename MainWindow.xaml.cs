using ApartmentChain.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace ApartmentChain
{
    public static class Session
    {
        public static bool IsAuthorized { get; set; } = false;
        public static string CurrentUserLogin { get; set; } = null;
    }

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (!(e.Content is Page page)) return;
            this.Title = page.Title;
        }
        private void LogoPhoto_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MainPage());
        }
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AuthPage());
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAuthorized || string.IsNullOrEmpty(Session.CurrentUserLogin))
            {
                MessageBox.Show("Вы не авторизованы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentUser = Entities.GetContext().Users.FirstOrDefault(u => u.Login == Session.CurrentUserLogin);

            if (currentUser == null)
            {
                MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MainFrame.Navigate(new ProfilePage(currentUser));
        }

        public void UpdateUI()
        {
            if (Session.IsAuthorized)
            {
                AuthButton.Visibility = Visibility.Collapsed;
                ProfilePanel.Visibility = Visibility.Visible;
            }
            else
            {
                AuthButton.Visibility = Visibility.Visible;
                ProfilePanel.Visibility = Visibility.Collapsed;
            }
        }

    }
}