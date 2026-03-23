using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class DestinationView : UserControl
    {
        private readonly DestinationService _service;

        public DestinationView()
        {
            InitializeComponent();
            _service = new DestinationService();
            LoadDestinations();
        }

        private void LoadDestinations()
        {
            try
            {
                DataGridDestinations.ItemsSource = _service.GetAllDestinations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading destinations: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = TxtSearchName.Text.Trim();
                var country = TxtSearchCountry.Text.Trim();
                DataGridDestinations.ItemsSource = _service.SearchDestinations(name, country);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearchName.Clear();
            TxtSearchCountry.Clear();
            LoadDestinations();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dest = new Destination
                {
                    Name = TxtName.Text.Trim(),
                    Country = TxtCountry.Text.Trim(),
                    Region = TxtRegion.Text.Trim(),
                    Description = TxtDescription.Text.Trim()
                };

                _service.CreateDestination(dest);
                MessageBox.Show("Destination added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadDestinations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(TxtDestId.Text, out int id))
                {
                    var dest = new Destination
                    {
                        DestinationId = id,
                        Name = TxtName.Text.Trim(),
                        Country = TxtCountry.Text.Trim(),
                        Region = TxtRegion.Text.Trim(),
                        Description = TxtDescription.Text.Trim()
                    };

                    _service.UpdateDestination(dest);
                    MessageBox.Show("Destination updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadDestinations();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(TxtDestId.Text, out int id))
                {
                    var result = MessageBox.Show("Are you sure you want to delete this destination?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _service.DeleteDestination(id);
                        MessageBox.Show("Destination deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearForm();
                        LoadDestinations();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void DataGridDestinations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridDestinations.SelectedItem is Destination selected)
            {
                TxtDestId.Text = selected.DestinationId.ToString();
                TxtName.Text = selected.Name;
                TxtCountry.Text = selected.Country;
                TxtRegion.Text = selected.Region;
                TxtDescription.Text = selected.Description;

                BtnAdd.IsEnabled = true;
                BtnUpdate.IsEnabled = true;
                BtnDelete.IsEnabled = true;
            }
        }

        private void ClearForm()
        {
            TxtDestId.Clear();
            TxtName.Clear();
            TxtCountry.Clear();
            TxtRegion.Clear();
            TxtDescription.Clear();

            BtnAdd.IsEnabled = true;
            BtnUpdate.IsEnabled = false;
            BtnDelete.IsEnabled = false;
            DataGridDestinations.SelectedItem = null;
        }
    }
}
