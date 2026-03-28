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
            try
            {
                // Show logged-in user info
                if (AuthSession.Current != null)
                    TxtCurrentUser.Text = $"{AuthSession.Current.FullName}  [{AuthSession.Current.RoleName}]";

                // Hide Accounts item in ComboBox if not admin
                CbiAccounts.Visibility = AuthSession.CanManageAccounts
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                // Default screen - Statistics as requested
                MainContent.Content = new StatisticsView();
                CbManagement.SelectedIndex = 0; // Shows "Management" placeholder
            }
            catch (System.Exception ex)
            {
                System.IO.File.WriteAllText("crash_log_mainwindow.txt", ex.ToString());
                MessageBox.Show("Error displaying Main Window: " + ex.Message);
            }
        }

        private void CbManagement_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (CbManagement.SelectedIndex == 0) return; // Skip "Management" placeholder

            var selected = (System.Windows.Controls.ComboBoxItem)CbManagement.SelectedItem;
            if (selected == null) return;

            string content = selected.Content.ToString();
            switch (content)
            {
                case "Tours":
                    MainContent.Content = new TourView();
                    break;
                case "Vehicles":
                    MainContent.Content = new VehicleView();
                    break;
                case "Customers":
                    MainContent.Content = new CustomerView();
                    break;
                case "Accounts":
                    MainContent.Content = new AccountView();
                    break;
            }
        }

        private void BtnOpenAssignment_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new TourAssignmentView();

        private void BtnOpenBooking_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new BookingView();

        private void BtnOpenPayment_Click(object sender, RoutedEventArgs e)
            => NavigateToPayment();

        public void NavigateToPayment(int? bookingId = null)
        {
            MainContent.Content = new PaymentView(bookingId);
        }

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
    }
}
