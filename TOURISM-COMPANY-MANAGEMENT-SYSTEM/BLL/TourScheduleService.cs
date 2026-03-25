using System;
using System.Collections.Generic;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class TourScheduleService
    {
        private readonly TourScheduleRepository _repository = new TourScheduleRepository();
        private readonly TourTemplateRepository _templateRepo = new TourTemplateRepository();

        public List<TourSchedule> GetAllSchedules()
        {
            return _repository.GetAllSchedules();
        }

        public List<TourSchedule> GetSchedulesByTemplate(int templateId)
        {
            return _repository.GetSchedulesByTemplate(templateId);
        }

        public TourSchedule? GetScheduleById(int scheduleId)
        {
            return _repository.GetById(scheduleId);
        }

        public void CreateSchedule(TourSchedule schedule)
        {
            ValidateSchedule(schedule);
            schedule.CreatedAt = DateTime.Now;
            schedule.UpdatedAt = DateTime.Now;
            _repository.Add(schedule);
        }

        public void UpdateSchedule(TourSchedule schedule)
        {
            ValidateSchedule(schedule, isUpdate: true);
            var existing = _repository.GetById(schedule.ScheduleId);
            if (existing != null)
            {
                existing.DepartureDate = schedule.DepartureDate;
                existing.ReturnDate = schedule.ReturnDate;
                existing.AvailableSlots = schedule.AvailableSlots;
                existing.Status = schedule.Status;
                existing.UpdatedAt = DateTime.Now;
                _repository.Update(existing);
            }
        }

        public void DeleteSchedule(int scheduleId)
        {
            if (_repository.HasBookings(scheduleId))
            {
                throw new Exception("Cannot delete a schedule that has active bookings.");
            }
            _repository.Delete(scheduleId);
        }

        private void ValidateSchedule(TourSchedule schedule, bool isUpdate = false)
        {
            var errors = new List<string>();

            if (schedule.TourTemplateId <= 0)
                errors.Add("Tour Template is required.");

            if (schedule.AvailableSlots < 0)
                errors.Add("Available slots cannot be negative.");

            var template = _templateRepo.GetById(schedule.TourTemplateId);
            if (template == null)
            {
                errors.Add("Invalid Tour Template.");
            }
            else
            {
                // Calculate ReturnDate automatically based on DurationDays
                var d = schedule.DepartureDate.ToDateTime(TimeOnly.MinValue);
                schedule.ReturnDate = DateOnly.FromDateTime(d.AddDays(template.DurationDays - 1));

                // Date logic checks
                if (!isUpdate && schedule.DepartureDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    errors.Add("Departure Date cannot be in the past for new schedules.");
                }

                // Slot logic checks
                if (isUpdate)
                {
                    int bookedSlots = _repository.GetTotalBookedSlots(schedule.ScheduleId);
                    if (schedule.AvailableSlots < bookedSlots)
                    {
                        errors.Add($"Cannot update available slots to {schedule.AvailableSlots}. There are already {bookedSlots} active bookings.");
                    }
                    if (schedule.AvailableSlots > template.MaxCapacity)
                    {
                        errors.Add($"Available slots cannot exceed the tour template's maximum capacity ({template.MaxCapacity}).");
                    }
                }
                else
                {
                    if (schedule.AvailableSlots != template.MaxCapacity)
                    {
                        schedule.AvailableSlots = template.MaxCapacity;
                    }
                }
            }

            // Verify Overlap with existing schedules for the same template
            if (schedule.ReturnDate.HasValue && _repository.CheckOverlap(schedule.TourTemplateId, schedule.DepartureDate, schedule.ReturnDate.Value, isUpdate ? schedule.ScheduleId : (int?)null))
            {
                errors.Add("A schedule for this tour template already exists in this date range.");
            }

            if (errors.Count > 0)
            {
                throw new Exception("Validation Error(s):\n- " + string.Join("\n- ", errors));
            }
        }
    }
}
