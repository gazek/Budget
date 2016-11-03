namespace Budget.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccountModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        FinancialInstitutionId = c.Int(nullable: false),
                        Number = c.String(nullable: false),
                        Type = c.Int(nullable: false),
                        Description = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialInstitutionModels", t => t.FinancialInstitutionId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.Id, unique: true)
                .Index(t => t.UserId)
                .Index(t => t.FinancialInstitutionId);
            
            CreateTable(
                "dbo.FinancialInstitutionModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OfxFid = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        OfxUrl = c.String(nullable: false),
                        OfxOrg = c.String(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Username = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                        CLIENTUID = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.Id, unique: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.BalanceModels",
                c => new
                    {
                        AccountId = c.Int(nullable: false),
                        AsOfDate = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.AccountId, t.AsOfDate })
                .ForeignKey("dbo.AccountModels", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId, unique: true);
            
            CreateTable(
                "dbo.CategoryModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.Id, unique: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PayeeModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.Id, unique: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.TransactionModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        ReferenceValue = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OriginalPayeeName = c.String(nullable: false),
                        OriginalMemo = c.String(nullable: false),
                        DateAdded = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        TopPayeeId = c.Int(nullable: false),
                        TopMemo = c.String(),
                        CheckNum = c.Int(nullable: false),
                        LastEditDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AccountModels", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.Id, unique: true)
                .Index(t => new { t.AccountId, t.ReferenceValue, t.Date }, unique: true, name: "IX_AccountIdReferenceValueAndDate");
            
            CreateTable(
                "dbo.TransactionDetailModels",
                c => new
                    {
                        TransactionId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        SubCategoryId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransferTransactionId = c.Int(nullable: false),
                        Memo = c.String(),
                        LastEditDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.TransactionId, t.CategoryId, t.SubCategoryId })
                .ForeignKey("dbo.CategoryModels", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.SubCategoryModels", t => t.SubCategoryId, cascadeDelete: true)
                .ForeignKey("dbo.TransactionModels", t => t.TransactionId, cascadeDelete: true)
                .ForeignKey("dbo.TransactionModels", t => t.TransferTransactionId)
                .Index(t => t.TransactionId)
                .Index(t => t.CategoryId)
                .Index(t => t.SubCategoryId)
                .Index(t => t.TransferTransactionId);
            
            CreateTable(
                "dbo.SubCategoryModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryModels", t => t.CategoryId)
                .Index(t => t.Id, unique: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.ImportNameToPayeeModels",
                c => new
                    {
                        PayeeId = c.Int(nullable: false),
                        ImportName = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.PayeeId, t.ImportName })
                .ForeignKey("dbo.PayeeModels", t => t.PayeeId, cascadeDelete: true)
                .Index(t => t.PayeeId);
            
            CreateTable(
                "dbo.PayeeDefaultDetails",
                c => new
                    {
                        PayeeId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        SubCategoryId = c.Int(nullable: false),
                        Allocation = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PayeeId, t.CategoryId, t.SubCategoryId });
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ImportNameToPayeeModels", "PayeeId", "dbo.PayeeModels");
            DropForeignKey("dbo.TransactionDetailModels", "TransferTransactionId", "dbo.TransactionModels");
            DropForeignKey("dbo.TransactionDetailModels", "TransactionId", "dbo.TransactionModels");
            DropForeignKey("dbo.TransactionDetailModels", "SubCategoryId", "dbo.SubCategoryModels");
            DropForeignKey("dbo.SubCategoryModels", "CategoryId", "dbo.CategoryModels");
            DropForeignKey("dbo.TransactionDetailModels", "CategoryId", "dbo.CategoryModels");
            DropForeignKey("dbo.TransactionModels", "AccountId", "dbo.AccountModels");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.PayeeModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CategoryModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BalanceModels", "AccountId", "dbo.AccountModels");
            DropForeignKey("dbo.AccountModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AccountModels", "FinancialInstitutionId", "dbo.FinancialInstitutionModels");
            DropForeignKey("dbo.FinancialInstitutionModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.ImportNameToPayeeModels", new[] { "PayeeId" });
            DropIndex("dbo.SubCategoryModels", new[] { "CategoryId" });
            DropIndex("dbo.SubCategoryModels", new[] { "Id" });
            DropIndex("dbo.TransactionDetailModels", new[] { "TransferTransactionId" });
            DropIndex("dbo.TransactionDetailModels", new[] { "SubCategoryId" });
            DropIndex("dbo.TransactionDetailModels", new[] { "CategoryId" });
            DropIndex("dbo.TransactionDetailModels", new[] { "TransactionId" });
            DropIndex("dbo.TransactionModels", "IX_AccountIdReferenceValueAndDate");
            DropIndex("dbo.TransactionModels", new[] { "Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.PayeeModels", new[] { "UserId" });
            DropIndex("dbo.PayeeModels", new[] { "Id" });
            DropIndex("dbo.CategoryModels", new[] { "UserId" });
            DropIndex("dbo.CategoryModels", new[] { "Id" });
            DropIndex("dbo.BalanceModels", new[] { "AccountId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.FinancialInstitutionModels", new[] { "UserId" });
            DropIndex("dbo.FinancialInstitutionModels", new[] { "Id" });
            DropIndex("dbo.AccountModels", new[] { "FinancialInstitutionId" });
            DropIndex("dbo.AccountModels", new[] { "UserId" });
            DropIndex("dbo.AccountModels", new[] { "Id" });
            DropTable("dbo.PayeeDefaultDetails");
            DropTable("dbo.ImportNameToPayeeModels");
            DropTable("dbo.SubCategoryModels");
            DropTable("dbo.TransactionDetailModels");
            DropTable("dbo.TransactionModels");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.PayeeModels");
            DropTable("dbo.CategoryModels");
            DropTable("dbo.BalanceModels");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.FinancialInstitutionModels");
            DropTable("dbo.AccountModels");
        }
    }
}
