using System;
using System.Collections.Generic;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class TourTemplateService
    {
        private readonly TourTemplateRepository _repository = new TourTemplateRepository();

        public List<TourTemplate> GetAllTemplates()
        {
            return _repository.GetAll();
        }

        public TourTemplate? GetTemplateById(int templateId)
        {
            return _repository.GetById(templateId);
        }

        public void CreateTemplate(TourTemplate template)
        {
            ValidateTemplate(template);
            template.CreatedAt = DateTime.Now;
            template.UpdatedAt = DateTime.Now;
            _repository.Add(template);
        }

        public void UpdateTemplate(TourTemplate template)
        {
            ValidateTemplate(template, isUpdate: true);
            var existing = _repository.GetById(template.TourTemplateId);
            if (existing != null)
            {
                existing.TourCode = template.TourCode;
                existing.TourName = template.TourName;
                existing.DestinationId = template.DestinationId;
                existing.DurationDays = template.DurationDays;
                existing.PricePerPerson = template.PricePerPerson;
                existing.MaxCapacity = template.MaxCapacity;
                existing.Description = template.Description;
                existing.ThumbnailUrl = template.ThumbnailUrl;
                existing.UpdatedAt = DateTime.Now;
                _repository.Update(existing);
            }
        }

        public void DeleteTemplate(int templateId)
        {
            _repository.Delete(templateId);
        }

        public List<TourTemplate> SearchTemplates(string? name, int? destId, decimal? minPrice, decimal? maxPrice)
        {
            return _repository.Search(name, destId, minPrice, maxPrice);
        }

        public List<KeyValuePair<int, string>> GetDestinations()
        {
            return _repository.GetDestinations();
        }

        private void ValidateTemplate(TourTemplate template, bool isUpdate = false)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(template.TourCode))
                errors.Add("Tour Code is required.");
            else if (template.TourCode.Length > 20)
                errors.Add("Tour Code cannot exceed 20 characters.");
            else if (_repository.ExistsByCode(template.TourCode, isUpdate ? template.TourTemplateId : (int?)null))
                errors.Add($"Tour Code '{template.TourCode}' already exists.");

            if (string.IsNullOrWhiteSpace(template.TourName))
                errors.Add("Tour Name is required.");
            else if (template.TourName.Length > 200)
                errors.Add("Tour Name cannot exceed 200 characters.");

            if (template.DestinationId <= 0)
                errors.Add("Destination is required.");

            if (template.DurationDays <= 0)
                errors.Add("Duration must be a positive number.");

            if (template.PricePerPerson <= 0)
                errors.Add("Price must be a positive number.");

            if (template.MaxCapacity <= 0)
                errors.Add("Capacity must be a positive number.");

            // Description and ThumbnailUrl rules if string limits apply
            if (!string.IsNullOrWhiteSpace(template.Description) && template.Description.Length > 2000)
                errors.Add("Description cannot exceed 2000 characters.");
            
            if (!string.IsNullOrWhiteSpace(template.ThumbnailUrl) && template.ThumbnailUrl.Length > 500)
                errors.Add("Thumbnail URL cannot exceed 500 characters.");

            if (errors.Count > 0)
            {
                throw new Exception("Validation Error(s):\n- " + string.Join("\n- ", errors));
            }
        }
    }
}
