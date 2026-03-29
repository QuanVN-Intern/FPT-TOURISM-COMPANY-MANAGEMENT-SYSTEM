using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM
{
    public partial class MainWindow : Window
    {
        // Constructor — wires up the Loaded event after XAML components are initialized
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        // Runs once the window is fully rendered and ready
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show logged-in user info in the top-right label
                if (AuthSession.Current != null)
                    TxtCurrentUser.Text = $"{AuthSession.Current.FullName}  [{AuthSession.Current.RoleName}]";

                // Apply role-based visibility rules to all nav elements
                ApplyRoleVisibility();

                // Default landing screen — drivers/guides go straight to assignments,
                // everyone else sees the statistics dashboard
                if (AuthSession.IsDriver || IsTourGuide())
                    MainContent.Content = new TourAssignmentView();
                else
                    MainContent.Content = new StatisticsView();

                // Reset the Management dropdown to its placeholder item
                CbManagement.SelectedIndex = 0;
            }
            catch (System.Exception ex)
            {
                // Log the full exception to a file for debugging, then show a user-friendly message
                System.IO.File.WriteAllText("crash_log_mainwindow.txt", ex.ToString());
                MessageBox.Show("Error displaying Main Window: " + ex.Message);
            }
        }

        // Local helper — checks for "Tour Guide" as stored in the DB
        // without touching the shared AuthSession class
        private static bool IsTourGuide()
            => AuthSession.Current?.RoleName == "Tour Guide";

        /// <summary>
        /// Show/hide every nav element based on the logged-in role.
        /// Roles: Admin | Manager | Staff | Guide | Driver
        /// </summary>
        private void ApplyRoleVisibility()
        {
            // ── Management ComboBox (entire control) ──────────────────────────
            // Hide completely for Driver and Tour Guide — they have no items to see
            // Admin  Manager  Staff  Tour Guide  Driver
            //   yes    yes     yes      no         no
            CbManagement.Visibility = (AuthSession.IsDriver || IsTourGuide())
                ? Visibility.Collapsed : Visibility.Visible;

            // ── Management ComboBox items ─────────────────────────────────────
            // Admin  Manager  Staff  Tour Guide  Driver
            CbiTours.Visibility     = Visibility.Visible;                          // yes    yes     yes    yes        yes
            CbiVehicles.Visibility  = AuthSession.CanSeeVehicles                   // yes    yes     yes    no         no
                ? Visibility.Visible : Visibility.Collapsed;
            CbiCustomers.Visibility = AuthSession.CanSeeCustomers                  // yes    yes     yes    no         no
                ? Visibility.Visible : Visibility.Collapsed;
            CbiAccounts.Visibility  = AuthSession.CanSeeAccounts                   // yes    no      no     no         no
                ? Visibility.Visible : Visibility.Collapsed;

            // ── Top-level nav buttons ─────────────────────────────────────────
            // Admin  Manager  Staff  Tour Guide  Driver
            BtnOpenBooking.Visibility    = AuthSession.CanSeeBookings              // yes    yes     yes    no         no
                ? Visibility.Visible : Visibility.Collapsed;
            BtnOpenAssignment.Visibility = AuthSession.CanSeeAssignments           // yes    yes     yes    yes        yes
                ? Visibility.Visible : Visibility.Collapsed;
            BtnOpenPayment.Visibility    = AuthSession.CanSeePayments              // yes    yes     yes    no         no
                ? Visibility.Visible : Visibility.Collapsed;
            BtnOpenStatistics.Visibility = AuthSession.CanSeeStatistics            // yes    yes     no     no         no
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Navigation handlers ───────────────────────────────────────────────

        // Fires when the user picks an item from the Management dropdown
        private void CbManagement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;                        // guard: ignore events before window is ready
            if (CbManagement.SelectedIndex == 0) return; // index 0 is the placeholder — nothing to navigate to

            var selected = (ComboBoxItem)CbManagement.SelectedItem;
            if (selected == null) return;

            // Load the appropriate view into the main content area
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

        // Navigate to the Tour Assignments view
        private void BtnOpenAssignment_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new TourAssignmentView();

        // Navigate to the Bookings view
        private void BtnOpenBooking_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new BookingView();

        // Navigate to the Payments view (no pre-selected booking)
        private void BtnOpenPayment_Click(object sender, RoutedEventArgs e)
            => NavigateToPayment();

        // Public entry point for navigating to payments — accepts an optional bookingId
        // so other views can deep-link directly to a specific booking's payment
        public void NavigateToPayment(int? bookingId = null)
            => MainContent.Content = new PaymentView(bookingId);

        // Navigate to the Statistics dashboard
        private void BtnOpenStatistics_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new StatisticsView();

        // Handles logout — confirms with the user, clears the session, and returns to the login screen
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return; // user cancelled

            AuthSession.Clear();       // wipe the current session data
            new LoginWindow().Show();  // show the login screen
            Close();                   // close this window
        }
    }
}