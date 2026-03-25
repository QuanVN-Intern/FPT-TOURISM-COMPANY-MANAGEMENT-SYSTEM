using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class VehicleBLL
    {
        private VehicleDAL _dal;

        public VehicleBLL()
        {
            _dal = new VehicleDAL();
        }

        public List<Vehicle> GetAllVehicles()
        {
            return _dal.GetAllVehicles();
        }

        public List<Vehicle> GetAllVehiclesWithDriver()
        {
            return _dal.GetAllVehiclesWithDriver();
        }

        public void AssignDriverToVehicle(int vehicleId, int? driverId)
        {
            // Validation: driver can only be assigned to one vehicle at a time
            if (driverId.HasValue)
            {
                var allVehicles = _dal.GetAllVehiclesWithDriver();
                var conflict = allVehicles.FirstOrDefault(v => v.DriverId == driverId && v.VehicleId != vehicleId);
                if (conflict != null)
                    throw new Exception($"Driver is already assigned to vehicle '{conflict.PlateNumber}'. Please unassign them first.");
            }
            _dal.UpdateDriverForVehicle(vehicleId, driverId);
        }

        public void AddVehicle(Vehicle vehicle)
        {
            _dal.AddVehicle(vehicle);
        }

        public void UpdateVehicle(Vehicle vehicle)
        {
            _dal.UpdateVehicle(vehicle);
        }

        public void DeleteVehicle(Vehicle vehicle)
        {
            _dal.DeleteVehicle(vehicle);
        }
        public List<Vehicle> SearchVehicles(string plateNumber)
        {
            var vehicles = _dal.GetAllVehicles();

            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return vehicles;
            }

            return vehicles
                .Where(v => v.PlateNumber.Contains(plateNumber, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
