using System;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class DriverManagerView : UserControl
    {
        private readonly DriverService _driverService = new DriverService();
        private readonly AccountService _accountService = new AccountService();

        public DriverManagerView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDrivers();
        }

        private void LoadDrivers()
        {
            try
            {
                DgDrivers.ItemsSource = _driverService.GetAllDrivers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading drivers: " + ex.Message);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDrivers();
        }

        private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            var selectedDriver = DgDrivers.SelectedItem as Account;
            if (selectedDriver == null)
            {
                MessageBox.Show("Please select a driver first.");
                return;
            }

            try
            {
                _accountService.ToggleActive(selectedDriver.AccountId, !selectedDriver.IsActive);
                LoadDrivers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
