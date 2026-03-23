using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class TourView : UserControl
    {
        private readonly TourService _tourService;
        private readonly List<KeyValuePair<int, string>> _destinations;
        private bool _isClearingForm = false;

        public TourView()
        {
            InitializeComponent();
            _tourService = new TourService();

            // Fetch actual destinations from DB if available, otherwise mock
            var repo = new TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL.TourRepository();
            _destinations = repo.GetDestinations();
            
            LoadComboBoxes();
            LoadData(); 
        }

        private void LoadComboBoxes()
        {
            CboDestination.ItemsSource = _destinations;
            CboDestination.DisplayMemberPath = "Value";
            CboDestination.SelectedValuePath = "Key";

            var searchDestinations = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "-- All Destinations --") };
            searchDestinations.AddRange(_destinations);

            CboSearchDestination.ItemsSource = searchDestinations;
            CboSearchDestination.DisplayMemberPath = "Value";
            CboSearchDestination.SelectedValuePath = "Key";
            CboSearchDestination.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                DataGridTours.ItemsSource = _tourService.GetAllTours();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGridTours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isClearingForm) return;

            if (DataGridTours.SelectedItem is Tour selectedTour)
            {
                TxtTourId.Text = selectedTour.TourId.ToString();
                TxtTourCode.Text = selectedTour.TourCode;
                TxtTourName.Text = selectedTour.TourName;
                CboDestination.SelectedValue = selectedTour.DestinationId;
                TxtDuration.Text = selectedTour.DurationDays.ToString();
                TxtPrice.Text = selectedTour.PricePerPerson.ToString("G29"); 
                TxtMaxCapacity.Text = selectedTour.MaxCapacity.ToString();
                DpDeparture.SelectedDate = selectedTour.DepartureDate.ToDateTime(TimeOnly.MinValue);

                TxtTourCode.IsEnabled = false;
                BtnAdd.IsEnabled = false;
                BtnUpdate.IsEnabled = true;
                BtnDelete.IsEnabled = true;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newTour = ExtractTourFromForm();
                _tourService.CreateTour(newTour);

                MessageBox.Show("New Tour added successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Business Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridTours.SelectedItem is not Tour selectedTour) return;

                var tourToUpdate = ExtractTourFromForm();
                tourToUpdate.TourId = selectedTour.TourId;
                tourToUpdate.TourCode = selectedTour.TourCode; 

                tourToUpdate.AvailableSlots = selectedTour.AvailableSlots;
                tourToUpdate.Status = selectedTour.Status;
                tourToUpdate.IsDeleted = selectedTour.IsDeleted;
                tourToUpdate.CreatedAt = selectedTour.CreatedAt;

                _tourService.UpdateTour(tourToUpdate);

                MessageBox.Show("Tour updated successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Business Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridTours.SelectedItem is not Tour selectedTour) return;

                var confirm = MessageBox.Show($"Are you sure you want to delete the Tour '{selectedTour.TourName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    _tourService.DeleteTour(selectedTour.TourId);

                    MessageBox.Show("Tour deleted successfully (Soft Delete)!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Business Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = TxtSearchName.Text.Trim();
                int? destId = CboSearchDestination.SelectedValue != null && (int)CboSearchDestination.SelectedValue != 0 
                                ? (int)CboSearchDestination.SelectedValue 
                                : null;

                decimal? minPrice = null;
                if (!string.IsNullOrWhiteSpace(TxtSearchMinPrice.Text) && decimal.TryParse(TxtSearchMinPrice.Text, out decimal min)) minPrice = min;

                decimal? maxPrice = null;
                if (!string.IsNullOrWhiteSpace(TxtSearchMaxPrice.Text) && decimal.TryParse(TxtSearchMaxPrice.Text, out decimal max)) maxPrice = max;

                DataGridTours.ItemsSource = _tourService.SearchTour(name, destId, minPrice, maxPrice);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            LoadData(); 
        }

        private void ClearForm()
        {
            _isClearingForm = true; 

            TxtTourId.Text = string.Empty;
            TxtTourCode.Text = string.Empty;
            TxtTourName.Text = string.Empty;
            CboDestination.SelectedIndex = -1;
            TxtDuration.Text = string.Empty;
            TxtPrice.Text = string.Empty;
            TxtMaxCapacity.Text = string.Empty;
            DpDeparture.SelectedDate = null;

            TxtSearchName.Text = string.Empty;
            CboSearchDestination.SelectedIndex = 0;
            TxtSearchMinPrice.Text = string.Empty;
            TxtSearchMaxPrice.Text = string.Empty;

            TxtTourCode.IsEnabled = true;
            BtnAdd.IsEnabled = true;
            BtnUpdate.IsEnabled = false;
            BtnDelete.IsEnabled = false;
            DataGridTours.SelectedItem = null;

            _isClearingForm = false;
        }

        private Tour ExtractTourFromForm()
        {
            var tour = new Tour
            {
                TourCode = TxtTourCode.Text.Trim(),
                TourName = TxtTourName.Text.Trim()
            };

            if (CboDestination.SelectedValue != null)
                tour.DestinationId = (int)CboDestination.SelectedValue;

            if (int.TryParse(TxtDuration.Text, out int dur))
                tour.DurationDays = dur;

            if (decimal.TryParse(TxtPrice.Text, out decimal price))
                tour.PricePerPerson = price;

            if (int.TryParse(TxtMaxCapacity.Text, out int maxCap))
                tour.MaxCapacity = maxCap;

            if (DpDeparture.SelectedDate.HasValue)
                tour.DepartureDate = DateOnly.FromDateTime(DpDeparture.SelectedDate.Value);
            else
                tour.DepartureDate = DateOnly.FromDateTime(DateTime.Now);

            return tour;
        }
    }
}
