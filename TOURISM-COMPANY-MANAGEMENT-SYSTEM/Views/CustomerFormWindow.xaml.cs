using System;
using System.Windows;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class CustomerFormWindow : Window
    {
        private readonly CustomerService _service = new CustomerService();
        private readonly Customer? _existing;  // null = Add mode

        public CustomerFormWindow(Customer? existing)
        {
            InitializeComponent();
            _existing = existing;

            if (existing != null)
            {
                TxtTitle.Text    = "Edit Customer";
                TxtFullName.Text = existing.FullName;
                TxtPhone.Text    = existing.Phone;
                TxtEmail.Text    = existing.Email    ?? "";
                DpDob.SelectedDate = existing.DateOfBirth;
                TxtAddress.Text  = existing.Address  ?? "";
                TxtPassport.Text = existing.PassportNo ?? "";
                TxtNotes.Text    = existing.Notes    ?? "";
            }
            else
            {
                TxtTitle.Text = "Add New Customer";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var c = new Customer
            {
                FullName    = TxtFullName.Text.Trim(),
                Phone       = TxtPhone.Text.Trim(),
                Email       = string.IsNullOrWhiteSpace(TxtEmail.Text) ? null : TxtEmail.Text.Trim(),
                DateOfBirth = DpDob.SelectedDate,
                Address     = string.IsNullOrWhiteSpace(TxtAddress.Text) ? null : TxtAddress.Text.Trim(),
                PassportNo  = string.IsNullOrWhiteSpace(TxtPassport.Text) ? null : TxtPassport.Text.Trim(),
                Notes       = string.IsNullOrWhiteSpace(TxtNotes.Text) ? null : TxtNotes.Text.Trim(),
            };

            try
            {
                if (_existing == null)
                {
                    _service.AddCustomer(c);
                    MessageBox.Show("Customer added successfully.", "Success",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    c.CustomerId = _existing.CustomerId;
                    _service.UpdateCustomer(c);
                    MessageBox.Show("Customer updated successfully.", "Success",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}