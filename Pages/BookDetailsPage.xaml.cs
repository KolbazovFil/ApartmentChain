using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ApartmentChain.Pages
{
    /// <summary>
    /// Страница бронирования и отображения деталей апартаментов.
    /// Позволяет пользователю просматривать информацию о квартире, оставлять отзывы,
    /// оформлять бронирование с выбранным методом оплаты.
    /// </summary>
    public partial class BookDetailsPage : Page
    {
        public Apartaments SelectedApartment { get; set; }

        public BookDetailsPage(Apartaments apartment)
        {
            InitializeComponent();

            SelectedApartment = apartment;
            DataContext = new BookDetailsViewModel(apartment);

            RatingComboBox.SelectedIndex = 3;
            PeopleCountComboBox.SelectedIndex = 0;

            LoadPaymentMethods();
        }

        private void LoadPaymentMethods()
        {
            var context = Entities.GetContext();
            var methods = context.MethodOfPay.ToList();

            MethodOfPayComboBox.ItemsSource = methods;
            MethodOfPayComboBox.DisplayMemberPath = "Method";
            MethodOfPayComboBox.SelectedValuePath = "ID";

            if (methods.Count > 0)
            {
                MethodOfPayComboBox.SelectedIndex = 0;
            }
        }
        private int GetCurrentCustomerID()
        {
            if (Session.IsAuthorized && !string.IsNullOrEmpty(Session.CurrentUserLogin))
            {
                var context = Entities.GetContext();

                var user = context.Users.FirstOrDefault(u => u.Login == Session.CurrentUserLogin);
                if (user != null)
                {
                    return user.ID;
                }
            }
            return 0;
        }
        private void ConfirmBookingButton_Click(object sender, RoutedEventArgs e)
        {
            var arrivalDate = ArivalDataPicker.SelectedDate;
            var departureDate = DepartureDataPicker.SelectedDate;
            int numberOfPeople = 1;

            if (PeopleCountComboBox.SelectedItem != null)
            {
                numberOfPeople = int.Parse((PeopleCountComboBox.SelectedItem as ComboBoxItem).Content.ToString());
            }

            if (arrivalDate == null || departureDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите даты заезда и выезда.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (arrivalDate >= departureDate)
            {
                MessageBox.Show("Дата заезда должна быть раньше даты выезда.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = Entities.GetContext();

            int daysCount = (departureDate.Value - arrivalDate.Value).Days;
            int selectedMethodID = (int)MethodOfPayComboBox.SelectedValue;
            int currentCustomerID = GetCurrentCustomerID();
            double pricePerDay = SelectedApartment.Price;
            double totalCost = pricePerDay * daysCount;

            var newBooking = new Booking
            {
                ApartmentID = SelectedApartment.ID,
                Arrival = arrivalDate.Value,
                Departure = departureDate.Value,
                CustomersID = currentCustomerID,
                TotalCost = totalCost,
                BookingStatusID = 2,
                AmountOfPeople = numberOfPeople
            };

            context.Booking.Add(newBooking);
            context.SaveChanges();

            var payment = new Payments
            {
                DateOFPayment = DateTime.Now,
                MethodID = selectedMethodID,
                BookingID = newBooking.ID
            };

            context.Payments.Add(payment);
            context.SaveChanges();

            MessageBox.Show("Отправлено на проверку, просьба следить за бронированием в профиле!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            if (NavigationService != null) NavigationService.Navigate(new MainPage());
        }
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var reviewsListBox = this.FindName("ReviewsListBox") as ListBox;
            if (reviewsListBox != null)
            {
                reviewsListBox.BringIntoView();
            }
        }
        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            NewReviewPanel.Visibility = Visibility.Visible;
            AddReviewButton.Visibility = Visibility.Collapsed;
        }
        private void CancelNewReview_Click(object sender, RoutedEventArgs e)
        {
            NewReviewPanel.Visibility = Visibility.Collapsed;
            NewReviewTextBox.Text = "";
            RatingComboBox.SelectedIndex = 3;
            AddReviewButton.Visibility = Visibility.Visible;
        }
        private void SendNewReview_Click(object sender, RoutedEventArgs e)
        {
            string reviewText = NewReviewTextBox.Text.Trim();
            if (string.IsNullOrEmpty(reviewText))
            {
                MessageBox.Show("Пожалуйста, введите текст отзыва");
                return;
            }

            SaveReview(reviewText);
            RefreshReviews();

            NewReviewPanel.Visibility = Visibility.Collapsed;
            NewReviewTextBox.Text = "";
            RatingComboBox.SelectedIndex = 3;
            AddReviewButton.Visibility = Visibility.Visible;
        }
        private void SaveReview(string reviewText)
        {
            var context = Entities.GetContext();
            int currentUserID = GetCurrentCustomerID();

            int rating = 0;
            if (RatingComboBox != null && RatingComboBox.SelectedItem != null)
            {
                rating = int.Parse(((ComboBoxItem)RatingComboBox.SelectedItem).Content.ToString());
            }

            var review = new Reviews
            {
                ApartmentID = SelectedApartment.ID,
                UserID = currentUserID,
                Review = reviewText,
                Rating = rating
            };

            context.Reviews.Add(review);
            context.SaveChanges();
            RefreshReviews();
        }

        private void RefreshReviews()
        {
            var context = Entities.GetContext();
            var reviews = context.Reviews.Where(r => r.ApartmentID == SelectedApartment.ID).ToList();
            if (reviews.Any())
            {
                double averageRating = reviews.Average(r => r.Rating);

                if (DataContext is BookDetailsViewModel vm)
                {
                    vm.Reviews = $"{averageRating:F1}/5";
                }

                var reviewsDisplay = reviews.Select(r => new ReviewDisplay
                {
                    User = r.Users?.Name ?? "Аноним",
                    Text = r.Review,
                    Rating = r.Rating
                }).ToList();

                if (DataContext is BookDetailsViewModel vm2)
                {
                    vm2.ReviewsList.Clear();
                    foreach (var r in reviewsDisplay)
                    {
                        vm2.ReviewsList.Add(r);
                    }
                    vm2.CountOfReviews = reviewsDisplay.Count;
                }
            }
            else
            {
                if (DataContext is BookDetailsViewModel vm)
                {
                    vm.Reviews = "Нет отзывов";
                    vm.ReviewsList.Clear();
                    vm.CountOfReviews = 0;
                }
            }
        }
    }
    /// <summary>
    /// Модель представления для страницы деталей бронирования.
    /// Управляет отображением информации о квартире, адресе, отзывах и фотографиях.
    /// </summary>
    public class BookDetailsViewModel : INotifyPropertyChanged
    {
        public string Countries { get; }
        public string Address { get; }
        public string Cities { get; }
        public decimal Price { get; }

        private string _reviews;
        public string Reviews
        {
            get => _reviews;
            set
            {
                if (_reviews != value)
                {
                    _reviews = value;
                    OnPropertyChanged(nameof(Reviews));
                }
            }
        }

        public Brush RatingBrush
        {
            get
            {
                if (Reviews == "Нет оценок" || string.IsNullOrEmpty(Reviews))
                    return Brushes.Gray;

                var parts = Reviews.Split('/');
                if (parts.Length > 0 && double.TryParse(parts[0], out double rating))
                {
                    if (rating >= 4.5)
                        return Brushes.Green;
                    else if (rating >= 3.0)
                        return Brushes.Orange;
                    else
                        return Brushes.Red;
                }
                return Brushes.Black;
            }
        }

        public ApartmentPhotosViewModel PhotosViewModel { get; }
        public ObservableCollection<ReviewDisplay> ReviewsList { get; }

        private int _countOfReviews;
        public int CountOfReviews
        {
            get => _countOfReviews;
            set
            {
                if (_countOfReviews != value)
                {
                    _countOfReviews = value;
                    OnPropertyChanged(nameof(CountOfReviews));
                    OnPropertyChanged(nameof(ReviewsCountText));
                }
            }
        }
        public string ReviewsCountText
        {
            get
            {
                int n = CountOfReviews;
                string word;
                if (n % 10 == 1 && n % 100 != 11)
                    word = "отзыв";
                else if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20))
                    word = "отзыва";
                else
                    word = "отзывов";

                return $"{n} {word}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (name == nameof(Reviews))
            {
                OnPropertyChanged(nameof(RatingBrush));
            }
        }

        public BookDetailsViewModel(Apartaments apartment)
        {
            var context = Entities.GetContext();
            var addr = context.ApartmentAddress.FirstOrDefault(a => a.ID == apartment.AddressID);
            if (addr != null)
            {
                var countryPart = !string.IsNullOrEmpty(addr.Countries?.Country) ? addr.Countries.Country : "";
                var cityPart = !string.IsNullOrEmpty(addr.Cities?.City) ? addr.Cities.City : "";
                var streetPart = addr.Streets?.Street ?? "";
                var buildingPart = addr.Building ?? "";

                var parts = new List<string>();
                if (!string.IsNullOrEmpty(countryPart)) parts.Add(countryPart);
                if (!string.IsNullOrEmpty(cityPart)) parts.Add(cityPart);
                if (!string.IsNullOrEmpty(streetPart)) parts.Add(streetPart);
                var addressString = string.Join(", ", parts) + $", д. {buildingPart}";

                Address = addressString;
            }

            var reviews = context.Reviews.Where(r => r.ApartmentID == apartment.ID).ToList();
            if (reviews.Any())
            {
                double averageRating = reviews.Average(r => r.Rating);
                Reviews = $"{averageRating:F1}/5";

                ReviewsList = new ObservableCollection<ReviewDisplay>(reviews.Select(r => new ReviewDisplay
                {
                    User = r.Users?.Name ?? "Аноним",
                    Text = r.Review,
                    Rating = r.Rating
                }));
            }
            else
            {
                Reviews = "Нет оценок";
                ReviewsList = new ObservableCollection<ReviewDisplay>();
            }

            CountOfReviews = ReviewsList.Count;

            Price = (decimal)apartment.Price;

            var photos = context.ApartmentPhotos.Where(p => p.ApartmentID == apartment.ID).Select(p => p.PhotoUrl).ToList();

            PhotosViewModel = new ApartmentPhotosViewModel
            {
                Photos = photos,
                CurrentIndex = 0
            };
        }
    }
    /// <summary>
    /// Отображение отдельного отзыва для UI.
    /// </summary>
    public class ReviewDisplay
    {
        public string User { get; set; }
        public string Text { get; set; }
        public double Rating { get; set; }
    }
}