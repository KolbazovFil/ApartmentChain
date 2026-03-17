using System;
using System.Collections.Generic;
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
    public partial class AboutAppPage : Page
    {
        public AboutAppPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var context = Entities.GetContext();
            var confirmedCount = context.Booking.Count(b => b.BookingStatus.Status == "Подтверждено");
            CountDeal.Text = $"Совершено сделок: {confirmedCount.ToString()}";
        }
    }
}
