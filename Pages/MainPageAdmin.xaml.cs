using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ApartmentChain.Pages
{
    public partial class MainPageAdmin : Page
    {
        public MainPageAdmin()
        {
            InitializeComponent();
            LoadData();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            {
                if (Visibility == Visibility.Visible)
                {
                    Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                }
            }
        }

        private void LoadData()
        {
            UsersDataGrid.ItemsSource = Entities.GetContext().Users.ToList();
            BookingsDataGrid.ItemsSource = Entities.GetContext().Booking.ToList();
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegPage((sender as Button).DataContext as Users));
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedUser = button?.DataContext as Users;

            if (selectedUser == null)
            {
                MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (selectedUser.Login == Session.CurrentUserLogin)
            {
                var selfDeleteResult = MessageBox.Show("Вы действительно хотите удалить свой профиль?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (selfDeleteResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = Entities.GetContext())
                        {
                            var userToDelete = context.Users.FirstOrDefault(u => u.ID == selectedUser.ID);
                            if (userToDelete != null)
                            {
                                context.Users.Remove(userToDelete);
                                context.SaveChanges();

                                Session.IsAuthorized = false;
                                Session.CurrentUserLogin = null;
                                (Application.Current.MainWindow as MainWindow)?.UpdateUI();
                                if (NavigationService != null) NavigationService.Navigate(new MainPage());

                                MessageBox.Show("Ваш профиль успешно удалён.", "Удаление профиля", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя {selectedUser.Surename} {selectedUser.Name}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = Entities.GetContext())
                    {
                        var userToDelete = context.Users.FirstOrDefault(u => u.ID == selectedUser.ID);
                        if (userToDelete != null)
                        {
                            context.Users.Remove(userToDelete);
                            context.SaveChanges();
                            (Application.Current.MainWindow as MainWindow)?.UpdateUI();


                            MessageBox.Show("Пользователь успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditBooking_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BookingStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}