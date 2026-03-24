using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    /// <summary>
    /// Interaction logic for VehicleView.xaml
    /// </summary>
    public partial class VehicleView : UserControl
    {
        private VehicleBLL _vehicleBll;

        public VehicleView()
        {
            InitializeComponent();
            _vehicleBll = new VehicleBLL();
            LoadData();
        }

        private void LoadData()
        {
            try 
            {
                dgVehicles.ItemsSource = _vehicleBll.GetAllVehicles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string plate = TxtSearchPlate.Text.Trim();
            dgVehicles.ItemsSource = _vehicleBll.SearchVehicles(plate);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            TxtSearchPlate.Text = string.Empty;
            LoadData();
        }

        private void dgVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgVehicles.SelectedItem is Vehicle selected)
            {
                txtId.Text = selected.VehicleId.ToString();
                txtPlate.Text = selected.PlateNumber;
                txtNote.Text = selected.Notes;
                
                // Select capacity in ComboBox
                foreach (ComboBoxItem item in CbCapacity.Items)
                {
                    if (item.Content.ToString() == selected.Capacity.ToString())
                    {
                        CbCapacity.SelectedItem = item;
                        break;
                    }
                }
                
                // Select status in ComboBox
                foreach (ComboBoxItem item in CbStatus.Items)
                {
                    if (item.Content.ToString() == selected.Status)
                    {
                        CbStatus.SelectedItem = item;
                        break;
                    }
                }
                
                BtnUpdate.IsEnabled = true;
                BtnDelete.IsEnabled = true;
                BtnAdd.IsEnabled = false;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vehicle = ExtractVehicleFromForm();
                if (vehicle == null) return;

                _vehicleBll.AddVehicle(vehicle);
                MessageBox.Show("Vehicle added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vehicle = ExtractVehicleFromForm();
                if (vehicle == null) return;

                if (int.TryParse(txtId.Text, out int id))
                {
                    vehicle.VehicleId = id;
                    _vehicleBll.UpdateVehicle(vehicle);
                    MessageBox.Show("Vehicle updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgVehicles.SelectedItem is Vehicle selected)
            {
                var result = MessageBox.Show($"Are you sure you want to delete vehicle {selected.PlateNumber}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _vehicleBll.DeleteVehicle(selected);
                        MessageBox.Show("Vehicle deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearForm();
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtId.Text = string.Empty;
            txtPlate.Text = string.Empty;
            CbCapacity.SelectedIndex = 0;
            CbStatus.SelectedIndex = 0;
            txtNote.Text = string.Empty;
            
            BtnUpdate.IsEnabled = false;
            BtnDelete.IsEnabled = false;
            BtnAdd.IsEnabled = true;
            dgVehicles.SelectedItem = null;
        }

        private Vehicle ExtractVehicleFromForm()
        {
            string plate = txtPlate.Text.Trim();
            if (string.IsNullOrEmpty(plate))
            {
                MessageBox.Show("Plate number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (CbCapacity.SelectedItem == null || !int.TryParse((CbCapacity.SelectedItem as ComboBoxItem)?.Content.ToString(), out int capacity))
            {
                MessageBox.Show("Please select a valid capacity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            string status = (CbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Available";

            return new Vehicle
            {
                PlateNumber = plate,
                Capacity = capacity,
                Status = status,
                Notes = txtNote.Text.Trim()
            };
        }
    }
}
