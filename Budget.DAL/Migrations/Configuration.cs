namespace Budget.DAL.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed partial class Configuration : DbMigrationsConfiguration<Budget.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
    }
}
