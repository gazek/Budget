using System.Data.Entity.Migrations;

namespace Budget.DAL.Migrations
{
    public sealed partial class Configuration : DbMigrationsConfiguration<Budget.DAL.ApplicationDbContext>
    {
        /*
         * add-migration -name <migrationName> -StartUpProjectName Budget.API -ProjectName Budget.DAL
         * update-database -StartUpProjectName Budget.API -ProjectName Budget.DAL
         * 
         */
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }
}
