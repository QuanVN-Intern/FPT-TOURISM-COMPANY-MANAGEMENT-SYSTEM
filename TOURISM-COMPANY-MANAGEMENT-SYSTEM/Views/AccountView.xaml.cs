using System;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class AccountView : UserControl
    {
        private readonly AccountService _service = new AccountService();

        public AccountView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Account management is Admin-only
            if (!AuthSession.CanManageAccounts)
            {
                MessageBox.Show("Access denied. Only Admins can manage accounts.",
                                "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            LoadAccounts();
        }

        private void LoadAccounts() => DgAccounts.ItemsSource = _service.GetAll();

        private Account? GetSelected()
        {
            var a = DgAccounts.SelectedItem as Account;
            if (a == null)
                MessageBox.Show("Please select an account first.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            return a;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new AccountFormWindow(null, _service.GetRoles());
            if (form.ShowDialog() == true) LoadAccounts();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelected();
            if (selected == null) return;

            var form = new AccountFormWindow(selected, _service.GetRoles());
            if (form.ShowDialog() == true) LoadAccounts();
        }

        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelected();
            if (selected == null) return;

            bool newState = !selected.IsActive;
            string action = newState ? "enable" : "disable";

            var confirm = MessageBox.Show(
                $"Are you sure you want to {action} account '{selected.Username}'?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _service.ToggleActive(selected.AccountId, newState);
                LoadAccounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelected();
            if (selected == null) return;

            var dialog = new PasswordResetWindow(selected.Username);
            if (dialog.ShowDialog() != true) return;

            try
            {
                _service.ResetPassword(selected.AccountId, dialog.NewPassword);
                MessageBox.Show("Password reset successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelected();
            if (selected == null) return;

            var confirm = MessageBox.Show(
                $"Delete account '{selected.Username}'? This cannot be undone.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _service.DeleteAccount(selected.AccountId);
                MessageBox.Show("Account deleted.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAccounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}