using System;
using System.Collections.Generic;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class DriverService
    {
        private readonly AccountRepository _repo = new AccountRepository();

        public List<Account> GetAllDrivers()
        {
            return _repo.GetAllDrivers();
        }

        public List<Account> GetActiveDrivers()
        {
            return _repo.GetAllDrivers().FindAll(d => d.IsActive);
        }

        public List<Account> GetTourGuides()
        {
            return _repo.GetTourGuides();
        }

        public void SaveDriver(Account driver)
        {
            if (string.IsNullOrWhiteSpace(driver.FullName))
                throw new Exception("Full name is required.");
            
            // In a real project, we'd check if the role is indeed 'Driver'
            // but for simplicity we assume the UI handles this or it's a new account.
            
            if (driver.AccountId <= 0)
            {
                driver.IsActive = true;
                _repo.Add(driver);
            }
            else
                _repo.Update(driver);
        }
    }
}
