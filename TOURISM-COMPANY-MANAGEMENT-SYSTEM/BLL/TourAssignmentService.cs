using System;
using System.Collections.Generic;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class TourAssignmentService
    {
        private readonly TourAssignmentDAL _dal = new TourAssignmentDAL();
        private readonly AccountRepository _accountRepo = new AccountRepository();

        public List<TourAssignment> GetAllAssignments()
        {
            return _dal.GetAll();
        }

        public void AssignDriver(int scheduleId, int accountId, int vehicleId)
        {
            // Validation: Driver must be Active
            var driver = _accountRepo.GetById(accountId);
            if (driver == null || !driver.IsActive)
                throw new Exception("Only active drivers can be assigned to a tour.");

            // Validation: Prevent duplicate assignment
            if (_dal.IsDuplicate(scheduleId, accountId))
                throw new Exception("This driver is already assigned to this tour.");

            var assignment = new TourAssignment
            {
                ScheduleId = scheduleId,
                AccountId = accountId,
                VehicleId = vehicleId
            };
            _dal.Add(assignment);
        }

        public void RemoveAssignment(int id)
        {
            _dal.Delete(id);
        }
    }
}
