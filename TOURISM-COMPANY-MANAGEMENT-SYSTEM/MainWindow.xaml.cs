using System.Windows;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Show logged-in user info
            if (AuthSession.Current != null)
                TxtCurrentUser.Text = $"{AuthSession.Current.FullName}  [{AuthSession.Current.RoleName}]";

            // Hide Accounts nếu không phải admin
            BtnOpenAccount.Visibility = AuthSession.CanManageAccounts
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Default screen
            MainContent.Content = new TourView();
        }

        private void BtnOpenTour_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new TourView();

        private void BtnOpenCustomer_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new CustomerView();

        private void BtnOpenVehicle_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new VehicleView();

        private void BtnOpenBooking_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new BookingView();

        private void BtnOpenAccount_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new AccountView();

        //private void BtnOpenPayment_Click(object sender, RoutedEventArgs e)
        //    => NavigateToPayment();

        //public void NavigateToPayment(int? bookingId = null)
        //{
        //    MainContent.Content = new PaymentView(bookingId);
        //}

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            AuthSession.Clear(); // clear session
            new LoginWindow().Show();
            Close();
        }

        private void BtnOpenStatistics_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new StatisticsView();
        }

        private void BtnOpenAssignment_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new TourAssignmentView();
    }
}