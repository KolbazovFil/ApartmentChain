using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ApartmentChain.Pages
{
    public partial class RegPage : Page
    {
        private Users _currentUser = new Users();
        private bool _isRegistrationMode;

        private List<Roles> _rolesList = new List<Roles>();

        public RegPage(Users user = null)
        {
            InitializeComponent();

            if (user == null)
            {
                _currentUser = new Users() { Birthday = DateTime.Today };
                _isRegistrationMode = true;
            }
            else
            {
                _currentUser = user;
                _isRegistrationMode = false;
            }

            DataContext = _currentUser;
            RolesComboBox.SelectedValue = _currentUser.RoleID;
            UpdateUI();
        }

        private void UpdateUI()
        {
            bool isAdmin = false;

            using (var db = new Entities())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == Session.CurrentUserLogin);
                if (user != null && user.RoleID == 1)
                {
                    isAdmin = true;
                }
                _rolesList = db.Roles.ToList();
                RolesComboBox.ItemsSource = _rolesList;
                RolesComboBox.DisplayMemberPath = "Role";
                RolesComboBox.SelectedValuePath = "ID";
                ValidPasswordTextBlock.Text = _currentUser.PasswordHash;
            }
            if (_isRegistrationMode)
            {
                RegButton.Visibility = Visibility.Visible;
                EditButton.Visibility = Visibility.Collapsed;
                RegHeadText.Visibility = Visibility.Visible;
                EditHeadText.Visibility = Visibility.Collapsed;
                RolesComboBox.Visibility = Visibility.Collapsed;
                RolesTextBox.Visibility = Visibility.Collapsed;
                RowDefinitionEight.Height = GridLength.Auto;
                Title = "Страница регистрации пользователя";
                ValidPasswordTextBlock.Visibility = Visibility.Collapsed;
                ValidPswdTxb.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (isAdmin)
                {
                    RegButton.Visibility = Visibility.Collapsed;
                    EditButton.Visibility = Visibility.Visible;
                    RegHeadText.Visibility = Visibility.Collapsed;
                    EditHeadText.Visibility = Visibility.Visible;
                    RolesComboBox.Visibility = Visibility.Visible;
                    RolesTextBox.Visibility = Visibility.Visible;
                    RowDefinitionEight.Height = new GridLength(50);
                    Title = "Страница редактирования пользователя";
                    ValidPasswordTextBlock.Visibility = Visibility.Visible;
                    ValidPswdTxb.Visibility = Visibility.Visible;

                    if (_currentUser != null)
                    {
                        RolesComboBox.SelectedValue = _currentUser.RoleID;
                    }
                }

                else
                {
                    if (_currentUser != null)
                    {
                        RegButton.Visibility = Visibility.Collapsed;
                        EditButton.Visibility = Visibility.Visible;
                        RegHeadText.Visibility = Visibility.Collapsed;
                        EditHeadText.Visibility = Visibility.Visible;
                        RolesComboBox.Visibility = Visibility.Collapsed;
                        RolesTextBox.Visibility = Visibility.Collapsed;
                        RowDefinitionEight.Height = GridLength.Auto;
                        Title = "Страница редактирования пользователя";
                        ValidPasswordTextBlock.Visibility = Visibility.Visible;
                        ValidPswdTxb.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public bool Registration(string name, string surname, string login, string password, string confirmPassword, string birthday, string phone)
        {
            DateTime? birthdayDate = null;
            if (!string.IsNullOrWhiteSpace(birthday))
            {
                if (DateTime.TryParseExact(birthday, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    birthdayDate = parsedDate;
                    DateTime today = DateTime.Today;
                    DateTime eighteenYearsAgo = today.AddYears(-18);
                    if (birthdayDate > eighteenYearsAgo)
                    {
                        MessageBox.Show("Вам должно быть больше 18 лет", "Возрастное ограничение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Некорректный формат даты, пример: 29.06.1995", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(login)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(confirmPassword)
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(surname)
                || string.IsNullOrWhiteSpace(birthday)
                || string.IsNullOrWhiteSpace(phone))
            {
                errors.AppendLine("Все поля должны быть заполнены");
            }

            if (login.Contains(" ")) errors.AppendLine("Поле для логина не должно содержать пробелы");

            if (confirmPassword != password) errors.AppendLine("Пароли не совпадают");

            if (!Regex.IsMatch(password, @"^(?=.*[0-9])(?=.*[a-zA-Z]).{6,}$"))
                errors.AppendLine("Пароль должен содержать минимум 6 символов, хотя бы одну цифру и только латинскую раскладку");

            if (!string.IsNullOrWhiteSpace(phone))
            {
                var cleanedPhone = Regex.Replace(phone, @"\D", "");

                if (cleanedPhone.StartsWith("8"))
                {
                    cleanedPhone = "7" + cleanedPhone.Substring(1);
                }

                if (!Regex.IsMatch(cleanedPhone, @"^7\d{10}$"))
                {
                    errors.AppendLine("Неверный формат номера телефона, пример: \"+71234567890\", или с 8");
                }
                else
                {
                    phone = "+" + cleanedPhone;
                }
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using (var db = new Entities())
            {
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Login == login);
                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return false;
                }

                Users userObject = new Users
                {
                    Name = name,
                    Surename = surname,
                    Login = login,
                    PasswordHash = password,
                    Birthday = birthdayDate ?? DateTime.MinValue,
                    PhoneNumber = phone,
                    RoleID = 2
                };

                db.Users.Add(userObject);
                db.SaveChanges();
                MessageBox.Show("Регистрация прошла успешно!");
                if (NavigationService != null) NavigationService.Navigate(new AuthPage());
                Clear();
                return true;
            }
        }

        public bool Edit(int userId, string name, string surname, string login, string password, string confirmPassword, string birthday, string phone, string role)
        {
            DateTime? birthdayDate = null;
            if (!string.IsNullOrWhiteSpace(birthday))
            {
                if (DateTime.TryParseExact(birthday, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    birthdayDate = parsedDate;
                    DateTime today = DateTime.Today;
                    DateTime eighteenYearsAgo = today.AddYears(-18);
                    if (birthdayDate > eighteenYearsAgo)
                    {
                        MessageBox.Show("Вам должно быть больше 18 лет", "Возрастное ограничение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Некорректный формат даты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(login)
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(surname)
                || string.IsNullOrWhiteSpace(birthday)
                || string.IsNullOrWhiteSpace(phone))
            {
                errors.AppendLine("Все поля должны быть заполнены");
            }

            if (login.Contains(" ")) errors.AppendLine("Поле для логина не должно содержать пробелы");

            string passwordToSave = _currentUser.PasswordHash;

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (confirmPassword != password)
                {
                    errors.AppendLine("Пароли не совпадают");
                }

                if (!Regex.IsMatch(password, @"^(?=.*[0-9])(?=.*[a-zA-Z]).{6,}$"))
                {
                    errors.AppendLine("Пароль должен содержать минимум 6 символов, хотя бы одну цифру и только латинскую раскладку");
                }

                passwordToSave = password;
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                var cleanedPhone = Regex.Replace(phone, @"\D", "");
                if (cleanedPhone.StartsWith("8"))
                {
                    cleanedPhone = "7" + cleanedPhone.Substring(1);
                }

                if (!Regex.IsMatch(cleanedPhone, @"^7\d{10}$"))
                {
                    errors.AppendLine("Неверный формат номера телефона, пример: \"+71234567890\" или с 8");
                }
                else
                {
                    phone = "+" + cleanedPhone;
                }
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using (var db = new Entities())
            {
                var user = db.Users.FirstOrDefault(u => u.ID == userId);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден");
                    return false;
                }

                if (user.Login != login)
                {
                    var existingUser = db.Users.AsNoTracking().FirstOrDefault(u => u.Login == login);
                    if (existingUser != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!");
                        return false;
                    }
                }

                user.Name = name;
                user.Surename = surname;
                user.Login = login;
                user.PasswordHash = passwordToSave;
                user.Birthday = birthdayDate ?? DateTime.MinValue;
                user.PhoneNumber = phone;
                if (user != null && user.RoleID == 1)
                    user.RoleID = (int)RolesComboBox.SelectedValue;
                else
                    user.RoleID = 2;

                db.SaveChanges();

                MessageBox.Show("Изменения успешно сохранены!");
                if (user != null && user.RoleID == 1)
                {
                    if (NavigationService != null) NavigationService.Navigate(new MainPageAdmin());
                }
                else
                {
                    if (NavigationService != null) NavigationService.Navigate(new MainPage());
                    Clear();
                }
                return true;
            }
        }

        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            Registration(
                NameTextBox.Text,
                SurnameTextBox.Text,
                LoginTextBox.Text,
                PasswordBox.Password,
                ConfirmPasswordBox.Password,
                BirthdayDatePicker.SelectedDate?.ToString("dd.MM.yyyy"),
                PhoneNumberTextBox.Text);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Edit(
                _currentUser.ID,
                NameTextBox.Text,
                SurnameTextBox.Text,
                LoginTextBox.Text,
                PasswordBox.Password,
                ConfirmPasswordBox.Password,
                BirthdayDatePicker.SelectedDate?.ToString("dd.MM.yyyy"),
                PhoneNumberTextBox.Text,
                RolesComboBox.Text);
        }

        private void Clear()
        {
            NameTextBox.Clear();
            SurnameTextBox.Clear();
            LoginTextBox.Clear();
            PasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            PhoneNumberTextBox.Clear();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
            Clear();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ShowPasswordCheckBox.IsChecked == true)
            {
                PasswordTextBlock.Text = PasswordBox.Password;
            }
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBlock.Visibility = Visibility.Visible;
            PasswordTextBlock.Text = PasswordBox.Password;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordTextBlock.Visibility =Visibility.Collapsed;
        }
    }
}