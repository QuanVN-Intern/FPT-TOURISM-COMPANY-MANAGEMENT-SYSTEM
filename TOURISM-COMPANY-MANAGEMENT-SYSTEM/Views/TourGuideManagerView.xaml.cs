using System;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class TourGuideManagerView : UserControl
    {
        private readonly TourGuideService _guideService = new TourGuideService();
        private readonly AccountService _accountService = new AccountService();

        public TourGuideManagerView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGuides();
        }

        private void LoadGuides()
        {
            try
            {
                DgGuides.ItemsSource = _guideService.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading tour guides: " + ex.Message);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadGuides();
        }

        private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            var selectedGuide = DgGuides.SelectedItem as Account;
            if (selectedGuide == null)
            {
                MessageBox.Show("Please select a tour guide first.");
                return;
            }

            try
            {
                _accountService.ToggleActive(selectedGuide.AccountId, !selectedGuide.IsActive);
                LoadGuides();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
