using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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