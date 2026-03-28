using System;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class TourScheduleView : UserControl
    {
        private readonly TourScheduleService _scheduleService;
        private readonly TourTemplate _template;
        private bool _isClearingForm = false;

        public TourScheduleView(TourTemplate template)
        {
            InitializeComponent();
            _scheduleService = new TourScheduleService();
            _template = template;

            TxtTitle.Text = $"✈ TOUR SCHEDULES: {_template.TourName} ({_template.TourCode})";
            SetStatusInCombo("Active");
            LoadData(); 
        }

        private void LoadData()
        {
            try
            {
                DataGridSchedules.ItemsSource = _scheduleService.GetSchedulesByTemplate(_template.TourTemplateId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading schedules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGridSchedules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isClearingForm) return;

            if (DataGridSchedules.SelectedItem is TourSchedule selectedSchedule)
            {
                TxtScheduleId.Text = selectedSchedule.ScheduleId.ToString();
                DpDeparture.SelectedDate = selectedSchedule.DepartureDate.ToDateTime(TimeOnly.MinValue);
                TxtSlots.Text = selectedSchedule.AvailableSlots.ToString();
                SetStatusInCombo(selectedSchedule.Status);

                BtnAdd.IsEnabled = true;
                BtnUpdate.IsEnabled = true;
                BtnDelete.IsEnabled = true;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newSchedule = ExtractScheduleFromForm();
                _scheduleService.CreateSchedule(newSchedule);

                MessageBox.Show("New Schedule added successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (DataGridSchedules.SelectedItem is not TourSchedule selectedSchedule) return;

                var scheduleToUpdate = ExtractScheduleFromForm();
                scheduleToUpdate.ScheduleId = selectedSchedule.ScheduleId;
                scheduleToUpdate.CreatedAt = selectedSchedule.CreatedAt;
                scheduleToUpdate.IsDeleted = selectedSchedule.IsDeleted;
                scheduleToUpdate.ReturnDate = selectedSchedule.ReturnDate; // Service will recalculate anyway

                _scheduleService.UpdateSchedule(scheduleToUpdate);

                MessageBox.Show("Schedule updated successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (DataGridSchedules.SelectedItem is not TourSchedule selectedSchedule) return;

                var confirm = MessageBox.Show("Are you sure you want to delete this Schedule?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    _scheduleService.DeleteSchedule(selectedSchedule.ScheduleId);

                    MessageBox.Show("Schedule deleted successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Business Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _isClearingForm = true; 

            TxtScheduleId.Text = string.Empty;
            DpDeparture.SelectedDate = null;
            TxtSlots.Text = string.Empty;
            SetStatusInCombo("Active");

            BtnAdd.IsEnabled = true;
            BtnUpdate.IsEnabled = false;
            BtnDelete.IsEnabled = false;
            DataGridSchedules.SelectedItem = null;

            _isClearingForm = false;
        }

        private TourSchedule ExtractScheduleFromForm()
        {
            var schedule = new TourSchedule
            {
                TourTemplateId = _template.TourTemplateId
            };

            if (DpDeparture.SelectedDate.HasValue)
                schedule.DepartureDate = DateOnly.FromDateTime(DpDeparture.SelectedDate.Value);
            else
                throw new Exception("Departure Date is required.");

            if (!string.IsNullOrWhiteSpace(TxtSlots.Text))
            {
                if (int.TryParse(TxtSlots.Text, out int slots))
                    schedule.AvailableSlots = slots;
                else
                    throw new Exception("Available Slots must be a number.");
            }
            else
            {
                schedule.AvailableSlots = _template.MaxCapacity; // Default to max
            }

            if (CboStatus.SelectedItem is ComboBoxItem item)
                schedule.Status = item.Content.ToString() ?? "Active";
            else
                schedule.Status = "Active";

            return schedule;
        }

        private void SetStatusInCombo(string status)
        {
            foreach (ComboBoxItem item in CboStatus.Items)
            {
                if (item.Content.ToString() == status)
                {
                    CboStatus.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnBackToTemplates_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContent.Content = new TourView();
            }
        }
    }
}
