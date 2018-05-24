namespace Budget.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transModelUnrequireOriginalPayeeName : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TransactionModels", "IX_AccountIdReferenceValueAndDate");
            AlterColumn("dbo.TransactionModels", "OriginalPayeeName", c => c.String(maxLength: 200));
            CreateIndex("dbo.TransactionModels", new[] { "AccountId", "ReferenceValue", "Date", "Amount", "OriginalPayeeName" }, unique: true, name: "IX_AccountIdReferenceValueAndDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TransactionModels", "IX_AccountIdReferenceValueAndDate");
            AlterColumn("dbo.TransactionModels", "OriginalPayeeName", c => c.String(nullable: false, maxLength: 200));
            CreateIndex("dbo.TransactionModels", new[] { "AccountId", "ReferenceValue", "Date", "Amount", "OriginalPayeeName" }, unique: true, name: "IX_AccountIdReferenceValueAndDate");
        }
    }
}
