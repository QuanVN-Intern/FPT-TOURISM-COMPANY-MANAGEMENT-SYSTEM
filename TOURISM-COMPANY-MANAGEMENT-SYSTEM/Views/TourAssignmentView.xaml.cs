using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class TourAssignmentView : UserControl
    {
        private readonly TourAssignmentService _assignmentService = new TourAssignmentService();
        private readonly TourService _tourService = new TourService();
        private readonly DriverService _driverService = new DriverService();
        private readonly VehicleBLL _vehicleBLL = new VehicleBLL();

        public TourAssignmentView()
        {
            InitializeComponent();
            DriverMgmtContent.Content = new DriverManagerView();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                CbTours.ItemsSource = _tourService.GetAllTours();
                CbDrivers.ItemsSource = _driverService.GetActiveDrivers();
                CbVehicles.ItemsSource = _vehicleBLL.GetAllVehicles();
                
                DgAssignments.ItemsSource = _assignmentService.GetAllAssignments();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void BtnAssign_Click(object sender, RoutedEventArgs e)
        {
            if (CbTours.SelectedValue == null || CbDrivers.SelectedValue == null || CbVehicles.SelectedValue == null)
            {
                MessageBox.Show("Please select a Tour, Driver, and Vehicle.");
                return;
            }

            try
            {
                int tourId = (int)CbTours.SelectedValue;
                int accountId = (int)CbDrivers.SelectedValue;
                int vehicleId = (int)CbVehicles.SelectedValue;

                _assignmentService.AssignDriver(tourId, accountId, vehicleId);
                MessageBox.Show("Driver assigned successfully!");
                DgAssignments.ItemsSource = _assignmentService.GetAllAssignments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDeleteAssignment_Click(object sender, RoutedEventArgs e)
        {
            var assignment = (sender as Button)?.DataContext as TourAssignment;
            if (assignment == null) return;

            var result = MessageBox.Show("Are you sure you want to remove this assignment?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _assignmentService.RemoveAssignment(assignment.AssignmentId);
                    DgAssignments.ItemsSource = _assignmentService.GetAllAssignments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
