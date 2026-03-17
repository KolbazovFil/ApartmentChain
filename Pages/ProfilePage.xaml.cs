using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

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
            UpdateUI();
        }

        private void UpdateUI()
        {
            var db = new Entities();
            var user = db.Users.FirstOrDefault(u => u.Login == Session.CurrentUserLogin);

            if (user != null && user.RoleID == 1)
                MainPageAdminButton.Visibility = Visibility.Visible;
            else
                MainPageAdminButton.Visibility = Visibility.Collapsed;
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
            NavigationService.Navigate(new RegPage((sender as Button).DataContext as Users));
        }

        private void ViewBookingButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null) NavigationService.Navigate(new BookingManagmentPage());
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            string currentUserLogin = Session.CurrentUserLogin;
            if (!string.IsNullOrEmpty(currentUserLogin))
            {
                var result = MessageBox.Show("Вы действительно хотите удалить свой профиль?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {

                    var context = Entities.GetContext();
                    var user = context.Users.FirstOrDefault(u => u.Login == currentUserLogin);

                    if (user != null)
                    {
                        var bookings = context.Booking.Where(b => b.CustomersID == user.ID).ToList();
                        context.Booking.RemoveRange(bookings);

                        var reviews = context.Reviews.Where(r => r.UserID == user.ID).ToList();
                        context.Reviews.RemoveRange(reviews);

                        var payments = context.Payments.Where(p => p.Booking != null && p.Booking.CustomersID == user.ID).ToList();
                        context.Payments.RemoveRange(payments);

                        context.Users.Remove(user);
                        context.SaveChanges();
                        MessageBox.Show("Профиль успешно удалён.", "Удаление профиля", MessageBoxButton.OK, MessageBoxImage.Information);
                        Session.IsAuthorized = false;
                        Session.CurrentUserLogin = null;
                        (Application.Current.MainWindow as MainWindow)?.UpdateUI();
                        if (NavigationService != null) NavigationService.Navigate(new MainPage());
                    }
                }
            }
        }

        private void MainPageAdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null) NavigationService.Navigate(new MainPageAdmin());
        }
    }
}