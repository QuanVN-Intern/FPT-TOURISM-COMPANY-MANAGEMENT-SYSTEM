using System;
using System.Collections.Generic;
using System.Windows;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class AccountFormWindow : Window
    {
        private readonly AccountService _service = new AccountService();
        private readonly Account? _existing;

        public AccountFormWindow(Account? existing)
        {
            InitializeComponent();
            _existing = existing;

            // Use assignable roles only (no Admin in the list)
            CmbRole.ItemsSource = _service.GetAssignableRoles();

            if (existing != null)
            {
                TxtTitle.Text            = "Edit Account";
                PanelUsername.Visibility = Visibility.Collapsed;
                PanelPassword.Visibility = Visibility.Collapsed;

                TxtFullName.Text      = existing.FullName;
                TxtEmail.Text         = existing.Email;
                DpDob.SelectedDate    = existing.DateOfBirth;
                CmbRole.SelectedValue = existing.RoleId;
            }
            else
            {
                TxtTitle.Text = "Add New Account";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_existing == null)
                {
                    var a = new Account
                    {
                        Username    = TxtUsername.Text.Trim(),
                        FullName    = TxtFullName.Text.Trim(),
                        Email       = TxtEmail.Text.Trim(),
                        RoleId      = (int)(CmbRole.SelectedValue ?? 0),
                        DateOfBirth = DpDob.SelectedDate,
                    };
                    _service.CreateAccount(a, TxtPassword.Password);
                    MessageBox.Show("Account created successfully.", "Success",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var a = new Account
                    {
                        AccountId   = _existing.AccountId,
                        FullName    = TxtFullName.Text.Trim(),
                        Email       = TxtEmail.Text.Trim(),
                        RoleId      = (int)(CmbRole.SelectedValue ?? 0),
                        IsActive    = _existing.IsActive,
                        DateOfBirth = DpDob.SelectedDate,
                    };
                    _service.UpdateAccount(a);
                    MessageBox.Show("Account updated successfully.", "Success",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}