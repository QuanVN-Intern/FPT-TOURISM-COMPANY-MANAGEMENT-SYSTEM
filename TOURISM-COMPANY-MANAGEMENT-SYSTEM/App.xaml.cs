using System.Windows;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.DispatcherUnhandledException += (s, args) =>
            {
                System.IO.File.WriteAllText("crash_log_app.txt", args.Exception.ToString());
                MessageBox.Show("CRITICAL CRASH: " + args.Exception.Message);
                args.Handled = true;
            };
            // Always start at the login screen
            new LoginWindow().Show();
        }
    }
}