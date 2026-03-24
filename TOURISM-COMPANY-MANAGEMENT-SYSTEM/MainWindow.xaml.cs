using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnOpenTour_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.TourView();
        }

        private void BtnOpenBooking_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.BookingView();
        }

        private void BtnOpenVehicle_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Views.VehicleView();
        }
    }
}