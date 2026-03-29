using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    /// <summary>
    /// Holds the currently logged-in account for the lifetime of the application.
    /// Access from anywhere via AuthSession.Current.
    /// </summary>
    public static class AuthSession
    {
        public static Account? Current { get; private set; }

        public static void SetUser(Account account) => Current = account;
        public static void Clear() => Current = null;

        // ── Role identity ─────────────────────────────────────────────────────
        public static bool IsLoggedIn => Current != null;
        public static bool IsAdmin => Current?.RoleName == "Admin";
        public static bool IsManager => Current?.RoleName == "Manager";
        public static bool IsDriver => Current?.RoleName == "Driver";
        public static bool IsReceptionist => Current?.RoleName == "Receptionist";
        public static bool IsGuide => Current?.RoleName == "Guide";

        // Convenience grouping
        public static bool IsAdminOrManager => IsAdmin || IsManager;

        // Read-only roles — cannot create/edit/delete anything
        public static bool IsReadOnlyUser => IsDriver || IsGuide || IsReceptionist;

        // ── Tour permissions ──────────────────────────────────────────────────
        // Everyone can view tours
        public static bool CanAddTour => IsAdminOrManager;
        public static bool CanEditTour => IsAdminOrManager;
        public static bool CanDeleteTour => IsAdminOrManager;

        // ── Customer permissions ──────────────────────────────────────────────
        // Driver cannot see customers at all
        // Receptionist and Guide can only VIEW — no add/edit/delete
        public static bool CanViewCustomer => IsAdmin || IsManager
                                             || IsReceptionist || IsGuide;
        public static bool CanAddCustomer => IsAdminOrManager;
        public static bool CanEditCustomer => IsAdminOrManager;
        public static bool CanDeleteCustomer => IsAdmin;

        // ── Account permissions ───────────────────────────────────────────────
        public static bool CanManageAccounts => IsAdmin;

        // ── Nav bar: Management ComboBox items ────────────────────────────────
        // Tours: everyone
        public static bool CanSeeTours => !IsDriver && !IsGuide;
        // Vehicles: Admin, Manager only (no Staff anymore)
        public static bool CanSeeVehicles => IsAdminOrManager;
        // Customers: not Driver
        public static bool CanSeeCustomers => IsAdmin || IsManager || IsReceptionist;
        // Accounts: Admin only
        public static bool CanSeeAccounts => IsAdmin;

        // ── Nav bar: Top-level buttons ────────────────────────────────────────
        // Bookings: Admin, Manager, Receptionist
        public static bool CanSeeBookings => IsAdmin || IsManager || IsReceptionist;
        // Assignments: everyone
        public static bool CanSeeAssignments => true;
        // Payments: Admin, Manager only
        public static bool CanSeePayments => IsAdminOrManager;
        // Statistics: Admin, Manager only
        public static bool CanSeeStatistics => IsAdminOrManager;
    }
}