using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.Text;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class TourAssignmentView : UserControl
    {
        private readonly TourGuideAssignmentService _guideAssignService = new TourGuideAssignmentService();
        private readonly TourScheduleService _scheduleService = new TourScheduleService();
        private readonly DriverService _driverService = new DriverService();
        private readonly VehicleBLL _vehicleBll = new VehicleBLL();
        private readonly TourGuideService _tourGuideService = new TourGuideService();

        public TourAssignmentView()
        {
            InitializeComponent();
            DriverMgmtContent.Content = new DriverManagerView();
            GuideMgmtContent.Content = new TourGuideManagerView();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => LoadAll();

        private void LoadAll()
        {
            try
            {
                // Tab 1 — Guide Assignment
                CbGuideSchedules.ItemsSource = _scheduleService.GetAllSchedules();
                CbGuides.ItemsSource = _tourGuideService.GetActive();
                DgGuideAssignments.ItemsSource = _guideAssignService.GetAll();

                // Tab 3 — Driver & Vehicle
                var vehicles = _vehicleBll.GetAllVehiclesWithDriver();
                CbVehiclesForDriver.ItemsSource = vehicles;
                DgVehicleDrivers.ItemsSource = vehicles;
                
                var activeDrivers = _driverService.GetActiveDrivers();
                CbDriversForVehicle.ItemsSource = activeDrivers;
                
                if (activeDrivers.Count == 0)
                {
                    MessageBox.Show("No active drivers found. Please add or activate drivers in the 'Manage Drivers' tab.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ════════════════════════════════════════════════
        // TAB 1 — Tour Guide Assignment
        // ════════════════════════════════════════════════
        private void BtnAssignGuide_Click(object sender, RoutedEventArgs e)
        {
            if (CbGuideSchedules.SelectedValue == null)
            { MessageBox.Show("Please select a Tour Schedule.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (CbGuides.SelectedValue == null)
            { MessageBox.Show("Please select a Tour Guide.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                _guideAssignService.AssignGuide(
                    (int)CbGuideSchedules.SelectedValue,
                    (int)CbGuides.SelectedValue);
                MessageBox.Show("Tour Guide assigned successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DgGuideAssignments.ItemsSource = _guideAssignService.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Assignment Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRemoveGuide_Click(object sender, RoutedEventArgs e)
        {
            var ga = (sender as Button)?.DataContext as TourGuideAssignment;
            if (ga == null) return;
            if (MessageBox.Show("Remove this guide assignment?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try { _guideAssignService.RemoveAssignment(ga.GuideAssignmentId); DgGuideAssignments.ItemsSource = _guideAssignService.GetAll(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        // ════════════════════════════════════════════════
        // TAB 3 — Driver & Vehicle
        // ════════════════════════════════════════════════
        private void CbVehiclesForDriver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbVehiclesForDriver.SelectedItem is Vehicle v)
                TxtCurrentDriver.Text = v.Driver?.FullName ?? "(none)";
            else
                TxtCurrentDriver.Text = string.Empty;
        }

        private void BtnAssignDriverToVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (CbVehiclesForDriver.SelectedValue == null)
            { MessageBox.Show("Please select a Vehicle.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (CbDriversForVehicle.SelectedValue == null)
            { MessageBox.Show("Please select a Driver.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                int vehicleId = (int)CbVehiclesForDriver.SelectedValue;
                int driverId  = (int)CbDriversForVehicle.SelectedValue;
                _vehicleBll.AssignDriverToVehicle(vehicleId, driverId);
                MessageBox.Show("Driver assigned to vehicle successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshVehicleTab();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Assignment Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void BtnRemoveDriverFromVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (CbVehiclesForDriver.SelectedValue == null)
            { MessageBox.Show("Please select a Vehicle.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                _vehicleBll.AssignDriverToVehicle((int)CbVehiclesForDriver.SelectedValue, null);
                TxtCurrentDriver.Text = "(none)";
                RefreshVehicleTab();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void RefreshVehicleTab()
        {
            var v = _vehicleBll.GetAllVehiclesWithDriver();
            CbVehiclesForDriver.ItemsSource = v;
            DgVehicleDrivers.ItemsSource = v;
            CbDriversForVehicle.ItemsSource = _driverService.GetActiveDrivers();
            TxtCurrentDriver.Text = string.Empty;
        }

        private void RefreshGuideTab()
        {
            var guides = _tourGuideService.GetActive();
            CbGuides.ItemsSource = guides;
            // No need to update DgTourGuides anymore as it's inside TourGuideManagerView
        }
        // ════════════════════════════════════════════════
        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var b = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(b).ToLower();
        }
    }
}
