using System.Linq;
using System.Windows.Controls;

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
            var confirmedCount = context.Booking.Count(b => b.BookingStatus.Status == "Завершено");
            CountDeal.Text = $"Завершено сделок: {confirmedCount.ToString()}";
        }
    }
}
