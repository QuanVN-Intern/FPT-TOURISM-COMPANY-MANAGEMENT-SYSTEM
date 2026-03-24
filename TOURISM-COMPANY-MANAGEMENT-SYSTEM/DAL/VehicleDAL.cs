using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class VehicleDAL
    {
        public List<Vehicle> GetAllVehicles()
        {
            using (var context = new TravelCompanyDbContext())
            {
                return context.Vehicles.ToList();
            }
        }

        public void AddVehicle(Vehicle vehicle)
        {
            using (var context = new TravelCompanyDbContext())
            {
                context.Vehicles.Add(vehicle);
                context.SaveChanges();
            }
        }

        public void UpdateVehicle(Vehicle vehicle)
        {
            using (var context = new TravelCompanyDbContext())
            {
                context.Vehicles.Update(vehicle);
                context.SaveChanges();
            }
        }

        public void DeleteVehicle(Vehicle vehicle)
        {
            using (var context = new TravelCompanyDbContext())
            {
                context.Vehicles.Remove(vehicle);
                context.SaveChanges();
            }
        }
    }
}
