using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class TourTemplateRepository
    {
        private readonly TravelCompanyDbContext _context = new TravelCompanyDbContext();

        public void Add(TourTemplate template)
        {
            _context.TourTemplates.Add(template);
            _context.SaveChanges();
        }

        public void Update(TourTemplate template)
        {
            _context.TourTemplates.Update(template);
            _context.SaveChanges();
        }

        public void Delete(int templateId)
        {
            var template = _context.TourTemplates.Find(templateId);
            if (template != null)
            {
                template.IsDeleted = true;
                template.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public TourTemplate? GetById(int templateId)
        {
            return _context.TourTemplates
                .Include(t => t.Destination)
                .FirstOrDefault(t => t.TourTemplateId == templateId && !t.IsDeleted);
        }

        public List<TourTemplate> GetAll()
        {
            return _context.TourTemplates
                .Include(t => t.Destination)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public List<TourTemplate> Search(string? name, int? destId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.TourTemplates
                .Include(t => t.Destination)
                .Where(t => !t.IsDeleted);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(t => t.TourName.Contains(name));
            
            if (destId.HasValue && destId.Value > 0)
                query = query.Where(t => t.DestinationId == destId.Value);
            
            if (minPrice.HasValue)
                query = query.Where(t => t.PricePerPerson >= minPrice.Value);
            
            if (maxPrice.HasValue)
                query = query.Where(t => t.PricePerPerson <= maxPrice.Value);

            return query.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public bool ExistsByCode(string code, int? excludeId = null)
        {
            var query = _context.TourTemplates.Where(t => t.TourCode == code && !t.IsDeleted);
            if (excludeId.HasValue)
                query = query.Where(t => t.TourTemplateId != excludeId.Value);
            return query.Any();
        }
        
        public List<KeyValuePair<int, string>> GetDestinations()
        {
            return _context.Destinations
                .Where(d => !d.IsDeleted)
                .Select(d => new KeyValuePair<int, string>(d.DestinationId, d.Name))
                .ToList();
        }
    }
}
