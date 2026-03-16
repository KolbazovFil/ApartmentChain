using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            if (_currentUser != null)
            {
                var bookings = context.Booking
                    .Include(b => b.Apartaments.ApartmentAddress.Cities)
                    .Include(b => b.Apartaments.ApartmentName)
                    .Include(b => b.BookingStatus)
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
                    }).ToList();

                BookingsDataGrid.ItemsSource = bookings;
            }
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

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
                }).ToList();

            BookingsDataGrid.ItemsSource = bookings;
        }

        private void LoadStatusComboBox()
        {
            var context = Entities.GetContext();
            var statuses = context.BookingStatus.ToList();

            var statusList = new List<BookingStatus>();
            statusList.AddRange(statuses);

            FilterStatusComboBox.ItemsSource = statusList;
            FilterStatusComboBox.DisplayMemberPath = "Status";
            FilterStatusComboBox.SelectedValuePath = "ID";
            FilterStatusComboBox.SelectedIndex = 0;
        }
        private void EditBooking_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
