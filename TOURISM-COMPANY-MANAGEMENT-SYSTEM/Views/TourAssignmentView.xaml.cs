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

            // Hide tabs that the current role should not see
            ApplyTabVisibility();
        }

        // Local helper — checks for "Tour Guide" as stored in the DB
        // without touching the shared AuthSession class
        private static bool IsTourGuide()
            => AuthSession.Current?.RoleName == "Tour Guide";

        // Restricts which tabs are visible based on the logged-in role
        private void ApplyTabVisibility()
        {
            var tc = (TabControl)Content;

            // Cast each item to TabItem so we can set Visibility
            // Tab indexes:
            // 0 = Tour Guide Assignment
            // 1 = Manage Tour Guides
            // 2 = Driver & Vehicle
            // 3 = Manage Drivers

            var tabGuideAssignment = (TabItem)tc.Items[0];
            var tabManageGuides = (TabItem)tc.Items[1];
            var tabDriverVehicle = (TabItem)tc.Items[2];
            var tabManageDrivers = (TabItem)tc.Items[3];

            if (AuthSession.IsDriver)
            {
                // Driver sees only "Driver & Vehicle" tab
                tabGuideAssignment.Visibility = Visibility.Collapsed;
                tabManageGuides.Visibility    = Visibility.Collapsed;
                tabDriverVehicle.Visibility   = Visibility.Visible;
                tabManageDrivers.Visibility   = Visibility.Collapsed;
                tc.SelectedIndex = 2; // land on Driver & Vehicle
            }
            else if (IsTourGuide())
            {
                // Tour Guide sees only "Tour Guide Assignment" tab
                tabGuideAssignment.Visibility = Visibility.Visible;
                tabManageGuides.Visibility    = Visibility.Collapsed;
                tabDriverVehicle.Visibility   = Visibility.Collapsed;
                tabManageDrivers.Visibility   = Visibility.Collapsed;
                tc.SelectedIndex = 0; // land on Tour Guide Assignment
            }
            // Admin, Manager, Staff see all tabs — no changes needed
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
                    MessageBox.Show(
                        "No active drivers found. Please add or activate drivers in the 'Manage Drivers' tab.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ════════════════════════════════════════════════
        // TAB 1 — Tour Guide Assignment
        // ════════════════════════════════════════════════

        private void BtnAssignGuide_Click(object sender, RoutedEventArgs e)
        {
            if (CbGuideSchedules.SelectedValue == null)
            {
                MessageBox.Show("Please select a Tour Schedule.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbGuides.SelectedValue == null)
            {
                MessageBox.Show("Please select a Tour Guide.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _guideAssignService.AssignGuide(
                    (int)CbGuideSchedules.SelectedValue,
                    (int)CbGuides.SelectedValue);
                MessageBox.Show("Tour Guide assigned successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DgGuideAssignments.ItemsSource = _guideAssignService.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Assignment Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRemoveGuide_Click(object sender, RoutedEventArgs e)
        {
            var ga = (sender as Button)?.DataContext as TourGuideAssignment;
            if (ga == null) return;

            if (MessageBox.Show("Remove this guide assignment?", "Confirm",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _guideAssignService.RemoveAssignment(ga.GuideAssignmentId);
                    DgGuideAssignments.ItemsSource = _guideAssignService.GetAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ════════════════════════════════════════════════
        // TAB 3 — Driver & Vehicle
        // ════════════════════════════════════════════════

        // Updates the "Current Driver" textbox when a vehicle is selected
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
            {
                MessageBox.Show("Please select a Vehicle.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbDriversForVehicle.SelectedValue == null)
            {
                MessageBox.Show("Please select a Driver.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int vehicleId = (int)CbVehiclesForDriver.SelectedValue;
                int driverId = (int)CbDriversForVehicle.SelectedValue;
                _vehicleBll.AssignDriverToVehicle(vehicleId, driverId);
                MessageBox.Show("Driver assigned to vehicle successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshVehicleTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Assignment Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRemoveDriverFromVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (CbVehiclesForDriver.SelectedValue == null)
            {
                MessageBox.Show("Please select a Vehicle.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Passing null removes the driver from the vehicle
                _vehicleBll.AssignDriverToVehicle((int)CbVehiclesForDriver.SelectedValue, null);
                TxtCurrentDriver.Text = "(none)";
                RefreshVehicleTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Reloads all vehicle/driver data after an assign or remove action
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
        }

        // ════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════

        // SHA-256 hash utility — used for password operations
        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var b = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(b).ToLower();
        }
    }
}