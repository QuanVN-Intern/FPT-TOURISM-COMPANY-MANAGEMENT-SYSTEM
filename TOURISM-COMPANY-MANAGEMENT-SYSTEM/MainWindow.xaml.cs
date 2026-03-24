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
            // Show logged-in user info in nav bar
            if (AuthSession.Current != null)
                TxtCurrentUser.Text = $"{AuthSession.Current.FullName}  [{AuthSession.Current.RoleName}]";

            // Accounts section is Admin-only — hide button for everyone else
            BtnOpenAccount.Visibility = AuthSession.CanManageAccounts
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Default landing view
            MainContent.Content = new TourView();
        }

        private void BtnOpenTour_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new TourView();

        private void BtnOpenCustomer_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new CustomerView();

        private void BtnOpenAccount_Click(object sender, RoutedEventArgs e)
            => MainContent.Content = new AccountView();

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            AuthSession.Clear();          // wipe the session
            new LoginWindow().Show();
            Close();
        }
    }
}