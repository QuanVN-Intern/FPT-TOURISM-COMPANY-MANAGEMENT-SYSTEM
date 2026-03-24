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
        private Tour? _selectedTour;

        public BookingView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                dgBookings.ItemsSource = _bookingBll.GetAllBookings();
                CbCustomer.ItemsSource = _bookingBll.GetCustomers();
                CbTour.ItemsSource = _bookingBll.GetActiveTours();
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
                CbTour.SelectedValue = selected.TourId;
                TxtNumPersons.Text = selected.NumPersons.ToString();
                TxtNotes.Text = selected.Notes;
                
                // Select Status
                foreach (ComboBoxItem item in CbStatus.Items)
                {
                    if (item.Content.ToString() == selected.Status)
                    {
                        CbStatus.SelectedItem = item;
                        break;
                    }
                }

                BtnUpdate.IsEnabled = true;
                BtnAdd.IsEnabled = false;
            }
        }

        private void CbTour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTour = CbTour.SelectedItem as Tour;
            CalculateTotal();
        }

        private void TxtNumPersons_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            if (_selectedTour != null && int.TryParse(TxtNumPersons.Text, out int num))
            {
                decimal total = _selectedTour.PricePerPerson * num;
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
                if (CbCustomer.SelectedValue == null || CbTour.SelectedValue == null)
                {
                    MessageBox.Show("Please select customer and tour.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    TourId = (int)CbTour.SelectedValue,
                    NumPersons = num,
                    Notes = TxtNotes.Text.Trim(),
                    AccountId = 1 // Simplified: Use a default admin account ID
                };

                _bookingBll.AddBooking(booking);
                MessageBox.Show("Booking created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadData();
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
                    string status = (CbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Confirmed";
                    string? reason = status == "Cancelled" ? "User requested cancellation" : null;
                    
                    _bookingBll.UpdateBookingStatus(id, status, reason);
                    MessageBox.Show("Booking updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            CbTour.SelectedIndex = -1;
            TxtNumPersons.Text = string.Empty;
            TxtNotes.Text = string.Empty;
            CbStatus.SelectedIndex = 0;
            TbTotalAmount.Text = "0 VNĐ";
            
            BtnUpdate.IsEnabled = false;
            BtnAdd.IsEnabled = true;
            dgBookings.SelectedItem = null;
        }
    }
}
