using System.Windows;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class PasswordResetWindow : Window
    {
        public string NewPassword { get; private set; } = "";

        public PasswordResetWindow(string username)
        {
            InitializeComponent();
            TxtTitle.Text = $"Reset password for: {username}";
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            if (TxtNew.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters.", "Validation",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (TxtNew.Password != TxtConfirm.Password)
            {
                MessageBox.Show("Passwords do not match.", "Validation",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NewPassword  = TxtNew.Password;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}