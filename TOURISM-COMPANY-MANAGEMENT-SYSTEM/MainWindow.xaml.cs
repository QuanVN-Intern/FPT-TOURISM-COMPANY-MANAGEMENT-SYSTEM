using System.Windows;
using System.Windows.Controls;
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

                ApplyRoleVisibility();

                // Default landing screen
                if (AuthSession.IsDriver || AuthSession.IsGuide)
                    MainContent.Content = new TourAssignmentView(); // land on assignments
                else
                    MainContent.Content = new StatisticsView();

                CbManagement.SelectedIndex = 0; // reset to placeholder
            }
            catch (System.Exception ex)
            {
                System.IO.File.WriteAllText("crash_log_mainwindow.txt", ex.ToString());
                MessageBox.Show("Error displaying Main Window: " + ex.Message);
            }
        }

        /// <summary>
        /// Show/hide every nav element based on the logged-in role.
        /// </summary>
        private void ApplyRoleVisibility()
        {
            // ── Management ComboBox items ─────────────────────────────────────
            // Tours: everyone
            CbiTours.Visibility     = Visibility.Visible;
            // Vehicles: Admin, Manager, Staff
            CbiVehicles.Visibility  = AuthSession.CanSeeVehicles
                ? Visibility.Visible : Visibility.Collapsed;
            // Customers: not Driver
            CbiCustomers.Visibility = AuthSession.CanSeeCustomers
                ? Visibility.Visible : Visibility.Collapsed;
            // Accounts: Admin only
            CbiAccounts.Visibility  = AuthSession.CanSeeAccounts
                ? Visibility.Visible : Visibility.Collapsed;

            // ── Top-level nav buttons ─────────────────────────────────────────
            BtnOpenBooking.Visibility    = AuthSession.CanSeeBookings
                ? Visibility.Visible : Visibility.Collapsed;
            BtnOpenAssignment.Visibility = AuthSession.CanSeeAssignments
                ? Visibility.Visible : Visibility.Collapsed;
            BtnOpenPayment.Visibility    = AuthSession.CanSeePayments
                ? Visibility.Visible : Visibility.Collapsed;
            BtnOpenStatistics.Visibility = AuthSession.CanSeeStatistics
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Navigation handlers ───────────────────────────────────────────────

        private void CbManagement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (CbManagement.SelectedIndex == 0) return;

            var selected = (ComboBoxItem)CbManagement.SelectedItem;
            if (selected == null) return;

            switch (selected.Content.ToString())
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
            => MainContent.Content = new PaymentView(bookingId);

        private void BtnOpenStatistics_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new StatisticsView();

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            AuthSession.Clear();
            new LoginWindow().Show();
            Close();
        }
    }
}