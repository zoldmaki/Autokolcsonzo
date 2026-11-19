using Autokolcsonzo.Data;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Autokolcsonzo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var context = new AutoKolcsonzoContext();
            context.Database.EnsureCreated();   
            DbSeeder.Seed(context);             
        }
    }

}
