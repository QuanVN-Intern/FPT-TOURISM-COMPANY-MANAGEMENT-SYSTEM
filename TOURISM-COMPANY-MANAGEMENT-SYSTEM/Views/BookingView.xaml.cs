using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class BookingView : UserControl
    {
        private readonly BookingBLL _bookingBll = new BookingBLL();
        private TourSchedule? _selectedSchedule;

        public BookingView()
        {
            InitializeComponent();
            ApplyPermissions();
            LoadData();
        }

        private void ApplyPermissions()
        {
            
            bool canUpdate = AuthSession.IsManager;
            
           
            if (!canUpdate)
            {
                PanelStatus.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadData()
        {
            try
            {
                _bookingBll.AutoReleaseVehicles(); // Check and release old vehicles
                dgBookings.ItemsSource = _bookingBll.GetAllBookings();
                CbCustomer.ItemsSource = _bookingBll.GetCustomers();
                CbSchedule.ItemsSource = _bookingBll.GetActiveSchedules();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string kw = TxtSearch.Text.Trim();
            dgBookings.ItemsSource = _bookingBll.SearchBookings(kw);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = string.Empty;
            LoadData();
        }

        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgBookings.SelectedItem is Booking selected)
            {
                txtId.Text = selected.BookingId.ToString();
                CbCustomer.SelectedValue = selected.CustomerId;
                CbSchedule.SelectedValue = selected.ScheduleId;
                _selectedSchedule = selected.TourSchedule;
                UpdateTourDates();

                TxtNumPersons.Text = selected.NumPersons.ToString();
                TxtNotes.Text = selected.Notes;
                
                // Status selection logic removed as CbStatus is hidden

                BtnUpdate.IsEnabled = true;
                BtnAdd.IsEnabled = false;

                // Disable fields during update - Status only allowed
                CbCustomer.IsEnabled = false;
                CbSchedule.IsEnabled = false;
                TxtNumPersons.IsEnabled = false;
                TxtNotes.IsEnabled = true; // Still allow note updates
            }
        }

        private void CbSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSchedule = CbSchedule.SelectedItem as TourSchedule;
            CalculateTotal();
            UpdateTourDates();
        }

        private void UpdateTourDates()
        {
            if (_selectedSchedule != null)
            {
                TbDepartureDate.Text = _selectedSchedule.DepartureDate.ToString("dd/MM/yyyy");
                TbDepartureDate.Foreground = System.Windows.Media.Brushes.Black;

                if (_selectedSchedule.ReturnDate.HasValue)
                {
                    TbEndDate.Text = _selectedSchedule.ReturnDate.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    // Fallback to duration if return date not set
                    DateTime endDate = _selectedSchedule.DepartureDate.ToDateTime(TimeOnly.MinValue).AddDays(_selectedSchedule.TourTemplate.DurationDays);
                    TbEndDate.Text = endDate.ToString("dd/MM/yyyy");
                }
                TbEndDate.Foreground = System.Windows.Media.Brushes.Black;
            }
            else
            {
                TbDepartureDate.Text = "Select tour";
                TbDepartureDate.Foreground = System.Windows.Media.Brushes.Gray;
                TbEndDate.Text = "Select tour";
                TbEndDate.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void TxtNumPersons_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotal();
            UpdateVehiclePreview();
        }

        private void UpdateVehiclePreview()
        {
            if (int.TryParse(TxtNumPersons.Text, out int num) && num > 0)
            {
                // If we are VIEWING/UPDATING an existing booking, show ACTUAL assigned vehicles
                if (dgBookings.SelectedItem is Booking selected)
                {
                    var actualVehicles = _bookingBll.GetVehiclesBySchedule(selected.ScheduleId);
                    if (actualVehicles.Any())
                    {
                        var actualNames = actualVehicles.Select(v => $"{v.PlateNumber} ({v.Capacity} seats)");
                        TbAssignedVehicles.Text = "Currently Assigned: " + string.Join(", ", actualNames);
                        return;
                    }
                }

                // Otherwise (ADD mode), show PREVIEW of what would be assigned
                var vehicles = _bookingBll.GetAvailableVehicles();
                if (!vehicles.Any())
                {
                    TbAssignedVehicles.Text = "No available vehicles!";
                    return;
                }

                int minCap = vehicles.Min(v => v.Capacity);
                int assignedCapacity = 0;
                var names = new List<string>();

                // Step 1: Best Single Fit (Gap <= minCap)
                var bestSingle = vehicles.Where(v => v.Capacity >= num && (v.Capacity - num) <= minCap)
                                         .OrderBy(v => v.Capacity).FirstOrDefault();
                if (bestSingle != null)
                {
                    names.Add($"{bestSingle.PlateNumber} ({bestSingle.Capacity} seats)");
                    assignedCapacity = bestSingle.Capacity;
                }
                else
                {
                    // Step 2: Multiple Selection
                    var temp = vehicles.OrderByDescending(v => v.Capacity).ToList();
                    int rem = num;
                    while (rem > 0 && temp.Any())
                    {
                        var v = temp.FirstOrDefault(x => x.Capacity <= rem) ?? temp.OrderBy(x => x.Capacity).First();
                        names.Add($"{v.PlateNumber} ({v.Capacity} seats)");
                        assignedCapacity += v.Capacity;
                        rem -= v.Capacity;
                        temp.Remove(v);
                    }
                }

                if (names.Any())
                {
                    TbAssignedVehicles.Text = "Auto-assign Preview: " + string.Join(", ", names);
                    if (assignedCapacity < num)
                        TbAssignedVehicles.Text += $" (Warning: Not enough capacity! Need {num - assignedCapacity} more)";
                }
                else
                {
                    TbAssignedVehicles.Text = "No available vehicles found!";
                }
            }
            else
            {
                TbAssignedVehicles.Text = "Enter number of persons to see assignment";
            }
        }

        private void CalculateTotal()
        {
            if (_selectedSchedule != null && int.TryParse(TxtNumPersons.Text, out int num))
            {
                decimal total = _selectedSchedule.TourTemplate.PricePerPerson * num;
                TbTotalAmount.Text = $"{total:N0} VNĐ";
            }
            else
            {
                TbTotalAmount.Text = "0 VNĐ";
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CbCustomer.SelectedValue == null || CbSchedule.SelectedValue == null)
                {
                    MessageBox.Show("Please select customer and schedule.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(TxtNumPersons.Text, out int num) || num <= 0)
                {
                    MessageBox.Show("Please enter a valid number of persons.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var booking = new Booking
                {
                    CustomerId = (int)CbCustomer.SelectedValue,
                    ScheduleId = (int)CbSchedule.SelectedValue,
                    BookingDate = _selectedSchedule.DepartureDate.ToDateTime(TimeOnly.MinValue),
                    NumPersons = num,
                    Notes = TxtNotes.Text.Trim(),
                    AccountId = AuthSession.Current?.AccountId ?? 1 // Use logged-in user or default
                };

                _bookingBll.AddBooking(booking);
                MessageBox.Show("Booking created successfully! Redirecting to payment...", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.NavigateToPayment(booking.BookingId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(txtId.Text, out int id))
                {
                    string notes = TxtNotes.Text.Trim();
                    _bookingBll.UpdateBookingInfo(id, notes);
                    MessageBox.Show("Booking information updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtId.Text = string.Empty;
            CbCustomer.SelectedIndex = -1;
            CbSchedule.SelectedIndex = -1;
            TxtNumPersons.Text = string.Empty;
            TxtNotes.Text = string.Empty;
            TbTotalAmount.Text = "0 VNĐ";
            TbAssignedVehicles.Text = "Auto-assigned on creation";
            UpdateTourDates();
            
            BtnUpdate.IsEnabled = false;
            BtnAdd.IsEnabled = true;
            dgBookings.SelectedItem = null;

            // Re-enable fields
            CbCustomer.IsEnabled = true;
            CbSchedule.IsEnabled = true;
            TxtNumPersons.IsEnabled = true;
            TxtNotes.IsEnabled = true;
        }
    }
}
