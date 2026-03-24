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

        // ── Role helpers ──────────────────────────────────────────────────────
        public static bool IsLoggedIn => Current != null;
        public static bool IsAdmin => Current?.RoleName == "Admin";
        public static bool IsManager => Current?.RoleName is "Admin" or "Manager";
        public static bool IsStaff => Current?.RoleName is "Admin" or "Manager" or "Staff";

        // Specific permission checks used throughout the UI
        public static bool CanDeleteCustomer => IsAdmin;
        public static bool CanEditCustomer => IsManager;   // Admin + Manager
        public static bool CanAddCustomer => IsStaff;     // Admin + Manager + Staff
        public static bool CanDeleteTour => IsManager;   // Admin + Manager (Staff cannot)
        public static bool CanManageAccounts => IsAdmin;
    }
}