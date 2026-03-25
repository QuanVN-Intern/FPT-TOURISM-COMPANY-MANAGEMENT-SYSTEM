using System;
using System.Collections.Generic;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class TourGuideAssignmentService
    {
        private readonly TourGuideAssignmentDAL _dal = new TourGuideAssignmentDAL();
        private readonly AccountRepository _accountRepo = new AccountRepository();
        private readonly TourScheduleRepository _scheduleRepo = new TourScheduleRepository();

        public List<TourGuideAssignment> GetAll() => _dal.GetAll();

        public void AssignGuide(int scheduleId, int accountId)
        {
            var errors = new List<string>();

            // 1. Guide must exist and be active
            var guide = _accountRepo.GetById(accountId);
            if (guide == null || !guide.IsActive)
                errors.Add("Tour Guide is not found or inactive.");

            // 2. Schedule must exist
            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null)
            {
                errors.Add("Selected Tour Schedule does not exist.");
            }
            else
            {
                // 3. Duplicate check
                if (_dal.IsDuplicate(scheduleId, accountId))
                    errors.Add("This guide is already assigned to the selected schedule.");

                // 4. Overlap check — guide cannot be on two overlapping tours
                if (schedule.ReturnDate.HasValue &&
                    _dal.HasOverlappingSchedule(accountId, schedule.DepartureDate, schedule.ReturnDate.Value, scheduleId))
                {
                    errors.Add("This guide already has another tour assignment whose dates overlap with this schedule.");
                }
            }

            if (errors.Count > 0)
                throw new Exception("Validation Error(s):\n- " + string.Join("\n- ", errors));

            _dal.Add(scheduleId, accountId);
        }

        public void RemoveAssignment(int id) => _dal.Delete(id);
    }
}
