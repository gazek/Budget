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
                        FinancialInstitutionId = c.Int(nullable: false),
                        Number = c.String(nullable: false, maxLength: 100),
                        RoutingNumber = c.Int(nullable: false),
                        Name = c.String(maxLength: 150),
                        NameStylized = c.String(nullable: false),
                        Type = c.Int(nullable: false),
                        Description = c.String(maxLength: 400),
                        StartDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialInstitutionModels", t => t.FinancialInstitutionId, cascadeDelete: true)
                .Index(t => new { t.FinancialInstitutionId, t.Number }, unique: true, name: "IX_UserFinancialInstitutionAccountNumber");
            
            CreateTable(
                "dbo.BalanceModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        AsOfDate = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AccountModels", t => t.AccountId, cascadeDelete: true)
                .Index(t => new { t.AccountId, t.AsOfDate }, unique: true, name: "IX_AccountIdAsOfDate");
            
            CreateTable(
                "dbo.FinancialInstitutionModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        OfxFid = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        NameStylized = c.String(nullable: false),
                        OfxUrl = c.String(nullable: false),
                        OfxOrg = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 50),
                        PasswordHash = c.Binary(nullable: false),
                        CLIENTUID = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.OfxFid, t.Name }, unique: true, name: "IX_FIDFINameUserId");
            
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
                "dbo.TransactionModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        ReferenceValue = c.String(nullable: false, maxLength: 100),
                        Date = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OriginalPayeeName = c.String(nullable: false, maxLength: 200),
                        OriginalMemo = c.String(nullable: false, maxLength: 200),
                        DateAdded = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        CheckNum = c.Int(nullable: false),
                        LastEditDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AccountModels", t => t.AccountId, cascadeDelete: true)
                .Index(t => new { t.AccountId, t.ReferenceValue, t.Date, t.Amount, t.OriginalPayeeName }, unique: true, name: "IX_AccountIdReferenceValueAndDate");
            
            CreateTable(
                "dbo.TransactionDetailModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(nullable: false),
                        PayeeId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        SubCategoryId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransferMatchId = c.Int(),
                        Memo = c.String(maxLength: 400),
                        LastEditDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryModels", t => t.CategoryId)
                .ForeignKey("dbo.PayeeModels", t => t.PayeeId)
                .ForeignKey("dbo.SubCategoryModels", t => t.SubCategoryId)
                .ForeignKey("dbo.TransactionModels", t => t.TransactionId, cascadeDelete: true)
                .ForeignKey("dbo.TransactionModels", t => t.TransferMatchId)
                .Index(t => new { t.TransactionId, t.PayeeId, t.CategoryId, t.SubCategoryId }, unique: true, name: "IX_TransDetailTransPayeeCatSubUnique")
                .Index(t => t.TransferMatchId);
            
            CreateTable(
                "dbo.CategoryModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        UserId = c.String(nullable: false, maxLength: 128),
                        NameStylized = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.Name, t.UserId }, unique: true, name: "IX_NameUsderId");
            
            CreateTable(
                "dbo.SubCategoryModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        NameStylized = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryModels", t => t.CategoryId, cascadeDelete: true)
                .Index(t => new { t.CategoryId, t.Name }, unique: true, name: "IX_CategoryIdName");
            
            CreateTable(
                "dbo.PayeeModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        NameStylized = c.String(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.Name, t.UserId }, unique: true, name: "IX_NameUserId");
            
            CreateTable(
                "dbo.PayeeDefaultDetailModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PayeeId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        SubCategoryId = c.Int(nullable: false),
                        Allocation = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryModels", t => t.CategoryId)
                .ForeignKey("dbo.PayeeModels", t => t.PayeeId, cascadeDelete: true)
                .ForeignKey("dbo.SubCategoryModels", t => t.SubCategoryId)
                .Index(t => new { t.PayeeId, t.CategoryId, t.SubCategoryId }, unique: true, name: "IX_PayeeCatSubCatIds");
            
            CreateTable(
                "dbo.PayeeImportNameModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PayeeId = c.Int(nullable: false),
                        ImportName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PayeeModels", t => t.PayeeId, cascadeDelete: true)
                .Index(t => new { t.PayeeId, t.ImportName }, unique: true, name: "IX_PayeeImportName");
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.TransactionDetailModels", "TransferMatchId", "dbo.TransactionModels");
            DropForeignKey("dbo.TransactionDetailModels", "TransactionId", "dbo.TransactionModels");
            DropForeignKey("dbo.TransactionDetailModels", "SubCategoryId", "dbo.SubCategoryModels");
            DropForeignKey("dbo.TransactionDetailModels", "PayeeId", "dbo.PayeeModels");
            DropForeignKey("dbo.PayeeModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PayeeImportNameModels", "PayeeId", "dbo.PayeeModels");
            DropForeignKey("dbo.PayeeDefaultDetailModels", "SubCategoryId", "dbo.SubCategoryModels");
            DropForeignKey("dbo.PayeeDefaultDetailModels", "PayeeId", "dbo.PayeeModels");
            DropForeignKey("dbo.PayeeDefaultDetailModels", "CategoryId", "dbo.CategoryModels");
            DropForeignKey("dbo.TransactionDetailModels", "CategoryId", "dbo.CategoryModels");
            DropForeignKey("dbo.CategoryModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SubCategoryModels", "CategoryId", "dbo.CategoryModels");
            DropForeignKey("dbo.TransactionModels", "AccountId", "dbo.AccountModels");
            DropForeignKey("dbo.AccountModels", "FinancialInstitutionId", "dbo.FinancialInstitutionModels");
            DropForeignKey("dbo.FinancialInstitutionModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BalanceModels", "AccountId", "dbo.AccountModels");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.PayeeImportNameModels", "IX_PayeeImportName");
            DropIndex("dbo.PayeeDefaultDetailModels", "IX_PayeeCatSubCatIds");
            DropIndex("dbo.PayeeModels", "IX_NameUserId");
            DropIndex("dbo.SubCategoryModels", "IX_CategoryIdName");
            DropIndex("dbo.CategoryModels", "IX_NameUsderId");
            DropIndex("dbo.TransactionDetailModels", new[] { "TransferMatchId" });
            DropIndex("dbo.TransactionDetailModels", "IX_TransDetailTransPayeeCatSubUnique");
            DropIndex("dbo.TransactionModels", "IX_AccountIdReferenceValueAndDate");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.FinancialInstitutionModels", "IX_FIDFINameUserId");
            DropIndex("dbo.BalanceModels", "IX_AccountIdAsOfDate");
            DropIndex("dbo.AccountModels", "IX_UserFinancialInstitutionAccountNumber");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.PayeeImportNameModels");
            DropTable("dbo.PayeeDefaultDetailModels");
            DropTable("dbo.PayeeModels");
            DropTable("dbo.SubCategoryModels");
            DropTable("dbo.CategoryModels");
            DropTable("dbo.TransactionDetailModels");
            DropTable("dbo.TransactionModels");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.FinancialInstitutionModels");
            DropTable("dbo.BalanceModels");
            DropTable("dbo.AccountModels");
        }
    }
}
