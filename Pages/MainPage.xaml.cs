using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ApartmentChain.Pages
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            LoadAllApartment();
        }
        private void LoadAllApartment()
        {
            var context = Entities.GetContext();

            string cityFilter = CityTextBox.Text.Trim().ToLower();

            double? minPrice = null;
            if (double.TryParse(MinPriceTextBox.Text.Replace(',', '.'), out double min))
            {
                minPrice = min;
            }

            double? maxPrice = null;
            if (double.TryParse(MaxPriceTextBox.Text.Replace(',', '.'), out double max))
            {
                maxPrice = max;
            }

            var query = context.Apartaments.AsQueryable();

            if (minPrice.HasValue)
            {
                query = query.Where(a => a.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(a => a.Price <= maxPrice.Value);
            }

            var list = query.Select(a => new
            {
                Id = a.ID,
                Cities = context.ApartmentAddress
                    .Where(addr => addr.ID == a.AddressID)
                    .Select(addr => context.Cities
                        .Where(r => r.ID == addr.CityID)
                        .Select(r => r.City + ", ")
                        .FirstOrDefault())
                    .FirstOrDefault(),
                Address = context.ApartmentAddress
                    .Where(addr => addr.ID == a.AddressID)
                    .Select(addr => addr.Streets.Street + ", д. " + addr.Building)
                    .FirstOrDefault(),
                Price = a.Price,
                Photos = context.ApartmentPhotos
                    .Where(photo => photo.ApartmentID == a.ID)
                    .Select(photo => photo.PhotoUrl)
                    .ToList(),

                PhotosViewModel = new ApartmentPhotosViewModel
                {
                    Photos = context.ApartmentPhotos
                        .Where(photo => photo.ApartmentID == a.ID)
                        .Select(photo => photo.PhotoUrl)
                        .ToList(),
                    CurrentIndex = 0
                }
            }).Where(item =>
                string.IsNullOrEmpty(cityFilter) ||
                (item.Cities != null && item.Cities.ToLower().Contains(cityFilter))
            ).ToList();

            ApartList.ItemsSource = list;
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
        private void ToBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAuthorized)
            {
                MessageBox.Show("Необходимо авторизобаться.", "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (sender is Button btn && btn.Tag is int apartmentId)
            {
                var context = Entities.GetContext();
                var apartment = context.Apartaments.FirstOrDefault(a => a.ID == apartmentId);
                if (apartment != null)
                {
                    var bookingPage = new BookDetailsPage(apartment);
                    NavigationService.Navigate(bookingPage);
                }
            }
        }
        private void CityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAllApartment();
        }
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAllApartment();
        }
        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            MinPriceTextBox.Text = string.Empty;
            MaxPriceTextBox.Text= string.Empty;
            CityTextBox.Text = string.Empty;
        }
    }

    public class ApartmentPhotosViewModel : INotifyPropertyChanged
    {
        public List<string> Photos { get; set; }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex != value && value >= 0 && value < Photos.Count)
                {
                    _currentIndex = value;
                    OnPropertyChanged(nameof(CurrentPhoto));
                }
            }
        }

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
                    OnPropertyChanged(nameof(RatingBrush));
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

        public string CurrentPhoto => Photos.Count > 0 ? Photos[CurrentIndex] : "../Resources/plug.png";

        public ICommand PrevPhotoCommand { get; }
        public ICommand NextPhotoCommand { get; }

        public ApartmentPhotosViewModel()
        {
            PrevPhotoCommand = new RelayCommand(_ => PrevPhoto());
            NextPhotoCommand = new RelayCommand(_ => NextPhoto());
            _currentIndex = 0;
        }

        private void PrevPhoto()
        {
            if (Photos.Count == 0) return;
            CurrentIndex = (CurrentIndex - 1 + Photos.Count) % Photos.Count;
        }

        private void NextPhoto()
        {
            if (Photos.Count == 0) return;
            CurrentIndex = (CurrentIndex + 1) % Photos.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}