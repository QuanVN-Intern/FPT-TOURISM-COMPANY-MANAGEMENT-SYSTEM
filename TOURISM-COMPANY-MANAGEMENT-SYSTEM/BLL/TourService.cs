using System;
using System.Collections.Generic;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class TourService
    {
        private readonly TourRepository _repository;

        public TourService()
        {
            _repository = new TourRepository();
        }

        public void CreateTour(Tour tour)
        {
            if (string.IsNullOrEmpty(tour.Status)) tour.Status = "Active";

            if (tour.Status != "Active")
                throw new Exception("Validation Error: New tours must be created with 'Active' status.");

            ValidateTour(tour, isUpdate: false);

            if (tour.AvailableSlots == 0)
            {
                tour.AvailableSlots = tour.MaxCapacity;
            }

            tour.CreatedAt = DateTime.Now;
            tour.UpdatedAt = DateTime.Now;

            _repository.Add(tour);
        }

        public void UpdateTour(Tour tour)
        {
            if (string.IsNullOrEmpty(tour.Status)) tour.Status = "Active";

            var existingTour = _repository.GetById(tour.TourId);
            if (existingTour == null)
                throw new Exception("Tour does not exist in the system.");

            // 1. Immutable status check
            if (existingTour.Status == "Completed" || existingTour.Status == "Cancelled")
            {
                // Allow update only if status is NOT being changed (though user shouldn't be able to edit other fields either)
                if (tour.Status != existingTour.Status)
                    throw new Exception($"Business Error: Cannot change status from {existingTour.Status}.");
                
                // If status is Completed/Cancelled, we block all other edits too as per user requirements
                throw new Exception($"Business Error: Tours in '{existingTour.Status}' status cannot be modified.");
            }

            // 2. Status Transition Validation
            if (tour.Status == "Completed")
            {
                if (tour.DepartureDate.ToDateTime(TimeOnly.MinValue) > DateTime.Now)
                    throw new Exception("Business Error: Cannot set status to 'Completed' because the Departure Date has not passed yet.");
            }

            var bookedSlots = _repository.GetTotalBookedSlots(tour.TourId);
            // Relaxing this: allow modification if status is the only thing changing? 
            // No, user said "Cannot modify a tour that already has bookings" in previous logic. 
            // I'll keep it but allow STATUS change if possible? 
            // Actually, if it has bookings, changing price or date is dangerous. 
            if (bookedSlots > 0 && (tour.PricePerPerson != existingTour.PricePerPerson || tour.DepartureDate != existingTour.DepartureDate))
                throw new Exception("Business Error: Cannot modify Price or Departure Date for a tour that already has bookings.");

            if (tour.MaxCapacity < bookedSlots)
                throw new Exception($"Business Error: MaxCapacity cannot be less than the number of booked slots ({bookedSlots}).");

            ValidateTour(tour, isUpdate: true);

            tour.UpdatedAt = DateTime.Now;
            _repository.Update(tour);
        }

        public void DeleteTour(int tourId)
        {
            var existingTour = _repository.GetById(tourId);
            if (existingTour == null)
                throw new Exception("Tour does not exist.");

            if (existingTour.Status == "Active")
                throw new Exception("Business Error: Cannot delete a tour that is in Active status.");

            if (_repository.HasBookings(tourId))
                throw new Exception("Business Error: Cannot delete a tour that already has bookings.");

            _repository.Delete(tourId);
        }

        public List<Tour> GetAllTours()
        {
            return _repository.GetAll();
        }

        public List<Tour> SearchTour(string? name, int? destId, decimal? minPrice, decimal? maxPrice)
        {
            return _repository.Search(name, destId, minPrice, maxPrice);
        }

        public void ValidateTour(Tour tour, bool isUpdate)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(tour.TourCode))
                errors.Add("TourCode cannot be empty.");
            else if (tour.TourCode.Length > 20)
                errors.Add("TourCode cannot exceed 20 characters.");
            else if (_repository.ExistsByCode(tour.TourCode, isUpdate ? tour.TourId : (int?)null))
                errors.Add($"TourCode '{tour.TourCode}' already exists in the system.");

            if (string.IsNullOrWhiteSpace(tour.TourName))
                errors.Add("TourName cannot be empty.");

            if (!_repository.DestinationExists(tour.DestinationId))
                errors.Add("The referenced Destination does not exist in the system.");

            if (tour.DurationDays <= 0 || tour.DurationDays > 365)
                errors.Add("DurationDays must be between 1 and 365.");

            if (tour.PricePerPerson <= 0)
                errors.Add("PricePerPerson must be greater than 0.");
            else if (tour.PricePerPerson > 1000000000m)
                errors.Add("PricePerPerson exceeds the maximum allowed limit.");

            if (tour.MaxCapacity <= 0)
                errors.Add("MaxCapacity must be greater than 0.");
            else if (tour.MaxCapacity > 1000)
                errors.Add("MaxCapacity cannot exceed 1000 people per tour.");

            if (tour.AvailableSlots < 0)
                errors.Add("AvailableSlots cannot be negative.");
            else if (tour.AvailableSlots > tour.MaxCapacity)
                errors.Add("AvailableSlots cannot exceed MaxCapacity.");

            if (!isUpdate && tour.DepartureDate.DayNumber < DateOnly.FromDateTime(DateTime.Now).DayNumber)
                errors.Add("DepartureDate cannot be in the past for new tours.");

            var validStatuses = new List<string> { "Active", "Inactive", "Completed", "Cancelled" };
            if (!validStatuses.Contains(tour.Status))
                errors.Add("Status must be one of the following: Active, Inactive, Completed, Cancelled.");

            if (errors.Count > 0)
            {
                throw new Exception("Validation Error(s):\n- " + string.Join("\n- ", errors));
            }
        }
        public int GetTotalBookedSlots(int tourId)
        {
            return _repository.GetTotalBookedSlots(tourId);
        }

    }
}
