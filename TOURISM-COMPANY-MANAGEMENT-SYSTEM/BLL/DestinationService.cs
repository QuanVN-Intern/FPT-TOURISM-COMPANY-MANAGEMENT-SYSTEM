using System;
using System.Collections.Generic;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class DestinationService
    {
        private readonly DestinationRepository _repository;

        public DestinationService()
        {
            _repository = new DestinationRepository();
        }

        public void CreateDestination(Destination dest)
        {
            ValidateDestination(dest, isUpdate: false);
            dest.CreatedAt = DateTime.Now;
            dest.UpdatedAt = DateTime.Now;
            dest.IsDeleted = false;
            _repository.Add(dest);
        }

        public void UpdateDestination(Destination dest)
        {
            var existing = _repository.GetById(dest.DestinationId);
            if (existing == null)
                throw new Exception("Destination not found.");

            ValidateDestination(dest, isUpdate: true);
            dest.UpdatedAt = DateTime.Now;
            _repository.Update(dest);
        }

        public void DeleteDestination(int id)
        {
            if (_repository.ExistsInTours(id))
                throw new Exception("Business Error: Cannot delete a destination that is currently referenced by one or more tours.");

            _repository.Delete(id);
        }

        public List<Destination> GetAllDestinations()
        {
            return _repository.GetAll();
        }

        public Destination? GetDestinationById(int id)
        {
            return _repository.GetById(id);
        }

        public List<Destination> SearchDestinations(string? name, string? country)
        {
            return _repository.Search(name, country);
        }

        public void ValidateDestination(Destination dest, bool isUpdate)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dest.Name))
                errors.Add("Destination name is required.");
            else if (dest.Name.Length > 100)
                errors.Add("Destination name cannot exceed 100 characters.");
            else if (_repository.ExistsByName(dest.Name, isUpdate ? dest.DestinationId : null))
                errors.Add($"Destination name '{dest.Name}' already exists.");

            if (string.IsNullOrWhiteSpace(dest.Country))
                errors.Add("Country is required.");
            else if (dest.Country.Length > 100)
                errors.Add("Country name cannot exceed 100 characters.");

            if (!string.IsNullOrWhiteSpace(dest.Region) && dest.Region.Length > 100)
                errors.Add("Region cannot exceed 100 characters.");

            if (errors.Count > 0)
            {
                throw new Exception("Validation Error(s):\n- " + string.Join("\n- ", errors));
            }
        }
    }
}
