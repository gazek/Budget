namespace Budget.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RoutingNumberAddedToAccount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AccountModels", "RoutingNumber", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AccountModels", "RoutingNumber");
        }
    }
}
