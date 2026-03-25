using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        public List<Vehicle> GetAllVehiclesWithDriver()
        {
            using var context = new TravelCompanyDbContext();
            return context.Vehicles
                .Include(v => v.Driver)
                .ToList();
        }

        public void UpdateDriverForVehicle(int vehicleId, int? driverId)
        {
            using var context = new TravelCompanyDbContext();
            var v = context.Vehicles.Find(vehicleId);
            if (v == null) throw new Exception("Vehicle not found.");
            v.DriverId = driverId;
            context.SaveChanges();
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
