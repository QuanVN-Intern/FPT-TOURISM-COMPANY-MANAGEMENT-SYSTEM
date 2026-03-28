using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class PaymentView : UserControl
    {
        private readonly PaymentBLL _paymentBll = new PaymentBLL();
        private List<Booking> _unpaidBookings;

        public PaymentView(int? bookingId = null)
        {
            InitializeComponent();
            LoadData();

            if (bookingId.HasValue)
            {
                CbBooking.SelectedValue = bookingId.Value;
            }
        }

        private void LoadData()
        {
            try
            {
                dgPayments.ItemsSource = _paymentBll.GetAllPayments();
                _unpaidBookings = _paymentBll.GetUnpaidBookings();
                CbBooking.ItemsSource = _unpaidBookings;
                
                // Set default payment method
                CbMethod.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgPayments.ItemsSource = _paymentBll.SearchPayments(TxtSearch.Text.Trim());
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            LoadData();
        }

        private void dgPayments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = dgPayments.SelectedItem as Payment;
            if (selected != null)
            {
                txtId.Text = selected.PaymentId.ToString();
                CbBooking.SelectedValue = selected.BookingId;
                TxtAmount.Text = selected.Amount.ToString();
                TxtNotes.Text = selected.Notes;
                
                // Select method in combo
                foreach (ComboBoxItem item in CbMethod.Items)
                {
                    if (item.Content.ToString() == selected.PaymentMethod)
                    {
                        CbMethod.SelectedItem = item;
                        break;
                    }
                }

                BtnProcess.IsEnabled = selected.Status == "Pending";
                BtnCreate.IsEnabled = false;

                // Lock fields on detail view
                CbBooking.IsEnabled = false;
                CbMethod.IsEnabled = false;
            }
        }

        private void CbBooking_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = CbBooking.SelectedItem as Booking;
            if (selected != null)
            {
                TbBookingInfo.Text = $"Tour: {selected.TourSchedule.TourTemplate.TourName}\nTotal Due: {selected.TotalAmount:N0} VNĐ";
                TxtAmount.Text = selected.TotalAmount.ToString();
            }
            else
            {
                TbBookingInfo.Text = "Select a booking to see details";
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CbBooking.SelectedValue == null)
                    throw new Exception("Please select a booking.");

                if (!decimal.TryParse(TxtAmount.Text, out decimal amount) || amount <= 0)
                    throw new Exception("Please enter a valid amount.");

                var payment = new Payment
                {
                    BookingId = (int)CbBooking.SelectedValue,
                    Amount = amount,
                    PaymentMethod = (CbMethod.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Cash",
                    Notes = TxtNotes.Text.Trim()
                };

                _paymentBll.CreatePayment(payment);
                MessageBox.Show("Payment registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(txtId.Text, out int id))
                {
                    if (_paymentBll.ProcessPayment(id))
                    {
                        MessageBox.Show("Payment processed successfully (Simulated)!", "Payment Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Payment Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtId.Clear();
            CbBooking.SelectedIndex = -1;
            TxtAmount.Clear();
            TxtNotes.Clear();
            BtnProcess.IsEnabled = false;
            BtnCreate.IsEnabled = true;
            TbBookingInfo.Text = "Select a booking to see details";

            // Re-enable fields for new entry
            CbBooking.IsEnabled = true;
            CbMethod.IsEnabled = true;
        }
    }
}
