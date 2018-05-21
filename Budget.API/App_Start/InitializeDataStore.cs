using Budget.DAL;
using System.Linq;

namespace Budget.API
{
    public static class InitializeDataStore
    {
        public static void ConfigureMigrations() 
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<ApplicationDbContext, DAL.Migrations.Configuration>());

            var configuration = new DAL.Migrations.Configuration();
            var migrator = new System.Data.Entity.Migrations.DbMigrator(configuration);
            if (migrator.GetPendingMigrations().Any())
            {
                migrator.Update();
            }
        }
    }
}
    