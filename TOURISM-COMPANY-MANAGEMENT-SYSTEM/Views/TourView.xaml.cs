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
        private readonly TourTemplateService _templateService;
        private readonly List<KeyValuePair<int, string>> _destinations;
        private bool _isClearingForm = false;

        public TourView()
        {
            InitializeComponent();
            _templateService = new TourTemplateService();

            var repo = new TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL.TourTemplateRepository();
            _destinations = repo.GetDestinations();
            
            LoadComboBoxes();
            LoadData(); 
            ClearForm(); // Initialize with next Tour Code
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
                DataGridTours.ItemsSource = _templateService.GetAllTemplates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGridTours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isClearingForm) return;

            if (DataGridTours.SelectedItem is TourTemplate selectedTemplate)
            {
                TxtTourId.Text = selectedTemplate.TourTemplateId.ToString();
                TxtTourCode.Text = selectedTemplate.TourCode;
                TxtTourName.Text = selectedTemplate.TourName;
                CboDestination.SelectedValue = selectedTemplate.DestinationId;
                TxtDuration.Text = selectedTemplate.DurationDays.ToString();
                TxtPrice.Text = selectedTemplate.PricePerPerson.ToString("G29"); 
                TxtMaxCapacity.Text = selectedTemplate.MaxCapacity.ToString();

                TxtTourCode.IsEnabled = true;
                BtnAdd.IsEnabled = true;
                BtnUpdate.IsEnabled = true;
                BtnDelete.IsEnabled = true;
                BtnManageSchedules.IsEnabled = true;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newTemplate = ExtractTemplateFromForm();
                _templateService.CreateTemplate(newTemplate);

                MessageBox.Show("New Tour Template added successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (DataGridTours.SelectedItem is not TourTemplate selectedTemplate) return;

                var templateToUpdate = ExtractTemplateFromForm();
                templateToUpdate.TourTemplateId = selectedTemplate.TourTemplateId;
                templateToUpdate.TourCode = selectedTemplate.TourCode; 

                templateToUpdate.IsDeleted = selectedTemplate.IsDeleted;
                templateToUpdate.CreatedAt = selectedTemplate.CreatedAt;

                _templateService.UpdateTemplate(templateToUpdate);

                MessageBox.Show("Tour Template updated successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (DataGridTours.SelectedItem is not TourTemplate selectedTemplate) return;

                var confirm = MessageBox.Show($"Are you sure you want to delete the Tour Template '{selectedTemplate.TourName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    _templateService.DeleteTemplate(selectedTemplate.TourTemplateId);

                    MessageBox.Show("Tour Template deleted successfully (Soft Delete)!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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

                DataGridTours.ItemsSource = _templateService.SearchTemplates(name, destId, minPrice, maxPrice);
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
            TxtTourCode.Text = _templateService.GenerateNextTourCode();
            TxtTourName.Text = string.Empty;
            CboDestination.SelectedIndex = -1;
            TxtDuration.Text = string.Empty;
            TxtPrice.Text = string.Empty;
            TxtMaxCapacity.Text = string.Empty;

            TxtSearchName.Text = string.Empty;
            CboSearchDestination.SelectedIndex = 0;
            TxtSearchMinPrice.Text = string.Empty;
            TxtSearchMaxPrice.Text = string.Empty;

            TxtTourCode.IsEnabled = true;
            BtnAdd.IsEnabled = true;
            BtnUpdate.IsEnabled = false;
            BtnDelete.IsEnabled = false;
            if (BtnManageSchedules != null) BtnManageSchedules.IsEnabled = false;
            DataGridTours.SelectedItem = null;

            _isClearingForm = false;
        }

        private TourTemplate ExtractTemplateFromForm()
        {
            var template = new TourTemplate
            {
                TourCode = TxtTourCode.Text.Trim(),
                TourName = TxtTourName.Text.Trim()
            };

            if (CboDestination.SelectedValue != null)
                template.DestinationId = (int)CboDestination.SelectedValue;
            else
                throw new Exception("Please select a Destination.");

            if (int.TryParse(TxtDuration.Text, out int dur))
                template.DurationDays = dur;
            else
                throw new Exception("Duration must be a valid integer number.");

            if (decimal.TryParse(TxtPrice.Text, out decimal price))
                template.PricePerPerson = price;
            else
                throw new Exception("Price must be a valid decimal number.");

            if (int.TryParse(TxtMaxCapacity.Text, out int maxCap))
                template.MaxCapacity = maxCap;
            else
                throw new Exception("Max Capacity must be a valid integer number.");

            return template;
        }

        private void BtnManageDestinations_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContent.Content = new DestinationView();
            }
        }

        private void BtnManageSchedules_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTours.SelectedItem is TourTemplate selectedTemplate)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainContent.Content = new TourScheduleView(selectedTemplate);
                }
            }
        }
    }
}
