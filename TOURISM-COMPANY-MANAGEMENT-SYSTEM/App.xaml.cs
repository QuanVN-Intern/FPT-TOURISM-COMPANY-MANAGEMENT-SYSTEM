using System.Windows;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Always start at the login screen
            new LoginWindow().Show();
        }
    }
}