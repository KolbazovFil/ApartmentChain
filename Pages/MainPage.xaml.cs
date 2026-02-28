using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ApartmentChain.Pages
{
    public static class Session
    {
        public static bool IsAuthorized { get; set; } = false;
        public static string CurrentUserLogin { get; set; } = null;
    }

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

            var list = context.Apartaments.Select(a => new
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
            }).ToList();

            ApartList.ItemsSource = list;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            {
                if (Visibility == Visibility.Visible)
                {
                    Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());

                    //if (Session.IsAuthorized)
                    //{
                    //    Authorization.Visibility = Visibility.Collapsed;
                    //    ProfilePanel.Visibility = Visibility.Visible;
                    //}
                    //else
                    //{
                    //    Authorization.Visibility = Visibility.Visible;
                    //    ProfilePanel.Visibility = Visibility.Collapsed;
                    //}
                }
            }
        }

        private void LogoPhoto_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы нажали на Логотип!", "Внимание");
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null) NavigationService.Navigate(new AuthPage());
        }
    }

    public class ApartmentPhotosViewModel : INotifyPropertyChanged
    {
        public List<string> Photos { get; set; } // список ссылок

        private int _currentIndex;
        public int CurrentIndex // текущий индекс
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

        public string CurrentPhoto => Photos.Count > 0 ? Photos[CurrentIndex] : "/Resources/plug.png";  // возвращает текущюу фото по индексу

        public ICommand PrevPhotoCommand { get; }
        public ICommand NextPhotoCommand { get; }

        public ApartmentPhotosViewModel()   // конструктор для создания команд
        {
            PrevPhotoCommand = new RelayCommand(_ => PrevPhoto());
            NextPhotoCommand = new RelayCommand(_ => NextPhoto());
            _currentIndex = 0;
        }

        private void PrevPhoto()    // метод переключения назад
        {
            if (Photos.Count == 0) return;
            CurrentIndex = (CurrentIndex - 1 + Photos.Count) % Photos.Count;
        }

        private void NextPhoto()    // метод переключения вперед
        {
            if (Photos.Count == 0) return;
            CurrentIndex = (CurrentIndex + 1) % Photos.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)   // для обновления интерфейса при изменениях
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class RelayCommand : ICommand
    {
        // Конструктор принимает делегат Action<object> execute, который вызывается при запуске команды.
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        // CanExecute определяет, можно ли выполнить команду; по умолчанию — всегда можно.
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}