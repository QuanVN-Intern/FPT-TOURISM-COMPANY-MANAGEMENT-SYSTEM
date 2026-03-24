using System.Windows;
using System.Windows.Input;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AccountService _service = new AccountService();

        public LoginWindow()
        {
            InitializeComponent();
            TxtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e) => TryLogin();

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) TryLogin();
        }

        private void TryLogin()
        {
            TxtError.Visibility = System.Windows.Visibility.Collapsed;

            var username = TxtUsername.Text.Trim();
            var password = TxtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            bool ok = _service.Login(username, password);
            if (!ok)
            {
                ShowError("Invalid username or password, or account is inactive.");
                TxtPassword.Clear();
                TxtUsername.Focus();
                return;
            }

            // Open main window and close login
            var main = new MainWindow();
            main.Show();
            Close();
        }

        private void ShowError(string msg)
        {
            TxtError.Text       = msg;
            TxtError.Visibility = System.Windows.Visibility.Visible;
        }
    }
}