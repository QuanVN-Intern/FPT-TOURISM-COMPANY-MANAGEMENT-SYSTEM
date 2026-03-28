using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class CustomerView : UserControl
    {
        private readonly CustomerService _service = new CustomerService();

        public CustomerView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            LoadCustomers();
        }

        private void ApplyPermissions()
        {
            // Add / Edit / Delete buttons respect role
            BtnAdd.IsEnabled    = AuthSession.CanAddCustomer;
            BtnEdit.IsEnabled   = AuthSession.CanEditCustomer;
            BtnDelete.IsEnabled = AuthSession.CanDeleteCustomer;

            // Visually dim disabled buttons
            BtnAdd.Opacity    = BtnAdd.IsEnabled ? 1.0 : 0.4;
            BtnEdit.Opacity   = BtnEdit.IsEnabled ? 1.0 : 0.4;
            BtnDelete.Opacity = BtnDelete.IsEnabled ? 1.0 : 0.4;

            // For Guide: show a read-only label
            if (AuthSession.IsReadOnlyUser)
            {
                TxtReadOnlyNotice.Visibility = Visibility.Visible;
            }
        }

        private void LoadCustomers(string keyword = "")
        {
            DgCustomers.ItemsSource = string.IsNullOrWhiteSpace(keyword)
                ? _service.GetAll()
                : _service.Search(keyword);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
            => LoadCustomers(TxtSearch.Text.Trim());

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new CustomerFormWindow(null);
            if (form.ShowDialog() == true) LoadCustomers();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgCustomers.SelectedItem as Customer;
            if (selected == null)
            {
                MessageBox.Show("Please select a customer to edit.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var form = new CustomerFormWindow(selected);
            if (form.ShowDialog() == true) LoadCustomers(TxtSearch.Text.Trim());
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgCustomers.SelectedItem as Customer;
            if (selected == null)
            {
                MessageBox.Show("Please select a customer to delete.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete '{selected.FullName}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _service.DeleteCustomer(selected.CustomerId);
                MessageBox.Show("Customer deleted successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCustomers(TxtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}