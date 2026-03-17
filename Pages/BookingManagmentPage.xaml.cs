using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ApartmentChain.Pages
{

    public partial class BookingManagmentPage : Page
    {
        private Users _currentUser = new Users();

        public BookingManagmentPage()
        {
            InitializeComponent();
            LoadCurrentUser();
            LoadData();
            UpdateBooking();
            LoadStatusComboBox();
            BookingsDataGrid.LoadingRow += BookingsDataGrid_LoadingRow;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            {
                if (Visibility == Visibility.Visible)
                {
                    var context = Entities.GetContext();
                    context.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                    LoadData();
                    UpdateBooking();
                }
            }
        }

        private void LoadData()
        {
            var context = Entities.GetContext();
            LoadCurrentUser();

            var bookings = context.Booking
                .Include(b => b.Apartaments.ApartmentAddress.Cities)
                .Include(b => b.Apartaments.ApartmentName)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payments)
                .Where(b => b.Users.ID == _currentUser.ID)
                .Select(b => new
                {
                    ID = b.ID,
                    Cities = b.Apartaments.ApartmentAddress.Cities.City,
                    ApartmentName = b.Apartaments.ApartmentName.Name,
                    Arrival = b.Arrival,
                    Departure = b.Departure,
                    TotalCost = b.TotalCost,
                    BookingStatus = b.BookingStatus.Status,
                    MethodOfPay = b.Payments.OrderByDescending(p => p.DateOFPayment).Select(p => p.MethodOfPay.Method).FirstOrDefault() ?? "Не оплачено"
                })
                .ToList();

            BookingsDataGrid.ItemsSource = bookings;
        }
        private void LoadCurrentUser()
        {
            var context = Entities.GetContext();

            string currentLogin = Session.CurrentUserLogin;

            if (!string.IsNullOrEmpty(currentLogin))
            {
                _currentUser = context.Users.FirstOrDefault(u => u.Login == currentLogin);
            }
        }

        private void FilterStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBooking();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateBooking();
        }

        private void UpdateBooking()
        {
            var context = Entities.GetContext();
            string searchText = SearchTextBox.Text.ToLower();
            int selectedStatusIndex = FilterStatusComboBox.SelectedIndex;

            var query = context.Booking
                .Include(b => b.Apartaments.ApartmentAddress.Cities)
                .Include(b => b.Apartaments.ApartmentName)
                .Include(b => b.BookingStatus)
                .Include(b => b.Payments)
                .Where(b => b.Users.ID == _currentUser.ID);

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(b =>
                    b.Apartaments.ApartmentAddress.Cities.City.ToLower().Contains(searchText) ||
                    b.Apartaments.ApartmentName.Name.ToLower().Contains(searchText));
            }

            if (selectedStatusIndex > 0)
            {
                var statuses = context.BookingStatus.ToList();
                if (statuses.Count > selectedStatusIndex)
                {
                    var selectedStatus = statuses[selectedStatusIndex];
                    query = query.Where(b => b.BookingStatus.ID == selectedStatus.ID);
                }
            }

            var bookings = query
                .Select(b => new
                {
                    ID = b.ID,
                    Cities = b.Apartaments.ApartmentAddress.Cities.City,
                    ApartmentName = b.Apartaments.ApartmentName.Name,
                    Arrival = b.Arrival,
                    Departure = b.Departure,
                    TotalCost = b.TotalCost,
                    BookingStatus = b.BookingStatus.Status,
                    MethodOfPay = b.Payments.OrderByDescending(p => p.DateOFPayment).Select(p => p.MethodOfPay.Method).FirstOrDefault() ?? "Не оплачено"
                }).ToList();

            BookingsDataGrid.ItemsSource = bookings;
        }
        private void LoadStatusComboBox()
        {
            var context = Entities.GetContext();
            var statuses = context.BookingStatus.ToList();
            FilterStatusComboBox.ItemsSource = statuses;
            FilterStatusComboBox.DisplayMemberPath = "Status";
            FilterStatusComboBox.SelectedValuePath = "ID";

            if (statuses.Any())
            {
                FilterStatusComboBox.SelectedIndex = 0;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BookingsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var item = e.Row.Item as dynamic;
            if (item == null) return;

            string status = item.BookingStatus;

            switch (status)
            {
                case "Отменено": e.Row.Foreground = Brushes.DarkRed; break;
                case "Подтверждено": e.Row.Foreground = Brushes.Green; break;
                case "Завершено": e.Row.Foreground = Brushes.DarkGreen; break;
                case "В обработке": e.Row.Foreground = Brushes.Gray; break;
                default: e.Row.Foreground = Brushes.Black; break;
            }
        }
    }
}
