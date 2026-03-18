using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ApartmentChain.Pages
{
    public partial class MainPageAdmin : Page
    {
        public List<BookingStatus> BookingStatuses { get; set; }
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
                    var context = Entities.GetContext();
                    context.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                    LoadData();
                }
            }
        }
        private void LoadData()
        {
            var context = Entities.GetContext();
            UsersDataGrid.ItemsSource = context.Users.ToList();
            BookingsDataGrid.ItemsSource = context.Booking.ToList();
            BookingStatuses = context.BookingStatus.ToList();
            DataContext = this;
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
                        var context = Entities.GetContext();

                        var userToDelete = context.Users.FirstOrDefault(u => u.ID == selectedUser.ID);
                        if (userToDelete != null)
                        {
                            var bookings = context.Booking.Where(b => b.CustomersID == userToDelete.ID).ToList();
                            context.Booking.RemoveRange(bookings);

                            var reviews = context.Reviews.Where(r => r.UserID == userToDelete.ID).ToList();
                            context.Reviews.RemoveRange(reviews);

                            var payments = context.Payments.Where(p => p.Booking != null && p.Booking.CustomersID == userToDelete.ID).ToList();
                            context.Payments.RemoveRange(payments);

                            context.Users.Remove(userToDelete);
                            context.SaveChanges();

                            Session.IsAuthorized = false;
                            Session.CurrentUserLogin = null;
                            (Application.Current.MainWindow as MainWindow)?.UpdateUI();
                            LoadData();
                            if (NavigationService != null) NavigationService.Navigate(new MainPage());

                            MessageBox.Show("Ваш профиль успешно удалён.", "Удаление профиля", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var context = Entities.GetContext();

                    var userToDelete = context.Users.FirstOrDefault(u => u.ID == selectedUser.ID);
                    if (userToDelete != null)
                    {
                        var bookings = context.Booking.Where(b => b.CustomersID == userToDelete.ID).ToList();
                        context.Booking.RemoveRange(bookings);

                        var reviews = context.Reviews.Where(r => r.UserID == userToDelete.ID).ToList();
                        context.Reviews.RemoveRange(reviews);

                        var payments = context.Payments.Where(p => p.Booking != null && p.Booking.CustomersID == userToDelete.ID).ToList();
                        context.Payments.RemoveRange(payments);

                        context.Users.Remove(userToDelete);
                        context.SaveChanges();
                        (Application.Current.MainWindow as MainWindow)?.UpdateUI();
                        LoadData();
                        MessageBox.Show("Пользователь успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedBooking = button?.DataContext as Booking;

            if (selectedBooking == null)
            {
                MessageBox.Show("Бронирование не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Удалить бронирование с ID: {selectedBooking.ID}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = Entities.GetContext();
                    var bookingToDelete = context.Booking.FirstOrDefault(u => u.ID == selectedBooking.ID);

                    if (bookingToDelete != null)
                    {
                        var payments = context.Payments.Where(p => p.BookingID == bookingToDelete.ID).ToList();
                        context.Payments.RemoveRange(payments);

                        context.Booking.Remove(bookingToDelete);
                        context.SaveChanges();
                        (Application.Current.MainWindow as MainWindow)?.UpdateUI();
                        LoadData();
                        MessageBox.Show("бронирование успешно удалёно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveBooking_Click(object sender, RoutedEventArgs e)
        {
            BookingsDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            BookingsDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

            var context = Entities.GetContext();

            try
            {
                context.SaveChanges();
                LoadData();

                foreach (var column in BookingsDataGrid.Columns)
                {
                    column.IsReadOnly = true;
                }

                BookingsDataGrid.IsReadOnly = true;
                EditBooking.Visibility = Visibility.Visible;
                SaveBooking.Visibility = Visibility.Collapsed;

                MessageBox.Show("Все изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        int[] editableColumnsIndex = { 2, 3, 6, 7, 8 };

        private void EditBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BookingsDataGrid.IsReadOnly = false;
                BookingsDataGrid.UpdateLayout();

                Style editableCellStyle = new Style(typeof(DataGridCell));

                var trigger = new DataTrigger()
                {
                    Binding = new Binding("IsReadOnly") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) },
                    Value = false
                };

                trigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightGreen));
                editableCellStyle.Triggers.Add(trigger);

                foreach (var column in BookingsDataGrid.Columns)
                {
                    int index = BookingsDataGrid.Columns.IndexOf(column);
                    if (editableColumnsIndex.Contains(index))
                    {
                        column.IsReadOnly = false;
                        column.CellStyle = editableCellStyle;
                    }
                    else
                    {
                        column.IsReadOnly = true;
                        column.CellStyle = null;
                    }
                }

                if (BookingsDataGrid.Columns.Count > 0)
                {
                    BookingsDataGrid.CurrentCell = new DataGridCellInfo(BookingsDataGrid.Items[0], BookingsDataGrid.Columns[0]);
                    BookingsDataGrid.BeginEdit();
                }

                EditBooking.Visibility = Visibility.Collapsed;
                SaveBooking.Visibility = Visibility.Visible;

                MessageBox.Show("- Вы вошли в режим редактирования бронирования;" +
                    "\n- Вы можете редактировать только подсвеченые колонки;" +
                    "\n- Не забудте сохранить изменения!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подготовке к редактированию: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}