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
                        FinancialInstitutionId = c.Int(nullable: false),
                        Number = c.String(nullable: false, maxLength: 100),
                        RoutingNumber = c.Int(nullable: false),
                        Name = c.String(maxLength: 150),
                        Type = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 400),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialInstitutionModels", t => t.FinancialInstitutionId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => new { t.UserId, t.FinancialInstitutionId, t.Number }, unique: true, name: "IX_UserFinancialInstitutionAccountNumber");
            
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
                        OfxFid = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        OfxUrl = c.String(nullable: false),
                        OfxOrg = c.String(nullable: false, maxLength: 50),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Username = c.String(nullable: false, maxLength: 50),
                        PasswordHash = c.Binary(nullable: false),
                        CLIENTUID = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => new { t.OfxFid, t.Name, t.UserId }, unique: true, name: "IX_FIDFINameUserId");
            
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
                .Index(t => new { t.AccountId, t.ReferenceValue, t.Date }, unique: true, name: "IX_AccountIdReferenceValueAndDate");
            
            CreateTable(
                "dbo.TransactionDetailModels",
                c => new
                    {
                        TransactionId = c.Int(nullable: false),
                        PayeeId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        SubCategoryId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransferTransactionId = c.Int(),
                        Memo = c.String(maxLength: 400),
                        LastEditDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.TransactionId, t.PayeeId, t.CategoryId, t.SubCategoryId })
                .ForeignKey("dbo.CategoryModels", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.PayeeModels", t => t.PayeeId, cascadeDelete: true)
                .ForeignKey("dbo.SubCategoryModels", t => t.SubCategoryId, cascadeDelete: true)
                .ForeignKey("dbo.TransactionModels", t => t.TransactionId, cascadeDelete: true)
                .ForeignKey("dbo.TransactionModels", t => t.TransferTransactionId)
                .Index(t => t.TransactionId)
                .Index(t => t.PayeeId)
                .Index(t => t.CategoryId)
                .Index(t => t.SubCategoryId)
                .Index(t => t.TransferTransactionId);
            
            CreateTable(
                "dbo.CategoryModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => new { t.Name, t.UserId }, unique: true, name: "IX_NameUsderId");
            
            CreateTable(
                "dbo.SubCategoryModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        CategoryModel_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryModels", t => t.CategoryId)
                .ForeignKey("dbo.CategoryModels", t => t.CategoryModel_Id)
                .Index(t => new { t.CategoryId, t.Name }, unique: true, name: "IX_CategoryIdName")
                .Index(t => t.CategoryModel_Id);
            
            CreateTable(
                "dbo.PayeeModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => new { t.Name, t.UserId }, unique: true, name: "IX_NameUserId");
            
            CreateTable(
                "dbo.PayeeDefaultDetailsModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PayeeId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        SubCategoryId = c.Int(nullable: false),
                        Allocation = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryModels", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.PayeeModels", t => t.PayeeId, cascadeDelete: true)
                .ForeignKey("dbo.SubCategoryModels", t => t.SubCategoryId, cascadeDelete: true)
                .Index(t => new { t.PayeeId, t.CategoryId, t.SubCategoryId }, unique: true, name: "IX_PayeeCatSubCatIds");
            
            CreateTable(
                "dbo.ImportNameToPayeeModels",
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
            DropForeignKey("dbo.AccountModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TransactionDetailModels", "TransferTransactionId", "dbo.TransactionModels");
            DropForeignKey("dbo.TransactionDetailModels", "TransactionId", "dbo.TransactionModels");
            DropForeignKey("dbo.TransactionDetailModels", "SubCategoryId", "dbo.SubCategoryModels");
            DropForeignKey("dbo.TransactionDetailModels", "PayeeId", "dbo.PayeeModels");
            DropForeignKey("dbo.PayeeModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ImportNameToPayeeModels", "PayeeId", "dbo.PayeeModels");
            DropForeignKey("dbo.PayeeDefaultDetailsModels", "SubCategoryId", "dbo.SubCategoryModels");
            DropForeignKey("dbo.PayeeDefaultDetailsModels", "PayeeId", "dbo.PayeeModels");
            DropForeignKey("dbo.PayeeDefaultDetailsModels", "CategoryId", "dbo.CategoryModels");
            DropForeignKey("dbo.TransactionDetailModels", "CategoryId", "dbo.CategoryModels");
            DropForeignKey("dbo.CategoryModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SubCategoryModels", "CategoryModel_Id", "dbo.CategoryModels");
            DropForeignKey("dbo.SubCategoryModels", "CategoryId", "dbo.CategoryModels");
            DropForeignKey("dbo.TransactionModels", "AccountId", "dbo.AccountModels");
            DropForeignKey("dbo.AccountModels", "FinancialInstitutionId", "dbo.FinancialInstitutionModels");
            DropForeignKey("dbo.FinancialInstitutionModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BalanceModels", "AccountId", "dbo.AccountModels");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.ImportNameToPayeeModels", "IX_PayeeImportName");
            DropIndex("dbo.PayeeDefaultDetailsModels", "IX_PayeeCatSubCatIds");
            DropIndex("dbo.PayeeModels", "IX_NameUserId");
            DropIndex("dbo.SubCategoryModels", new[] { "CategoryModel_Id" });
            DropIndex("dbo.SubCategoryModels", "IX_CategoryIdName");
            DropIndex("dbo.CategoryModels", "IX_NameUsderId");
            DropIndex("dbo.TransactionDetailModels", new[] { "TransferTransactionId" });
            DropIndex("dbo.TransactionDetailModels", new[] { "SubCategoryId" });
            DropIndex("dbo.TransactionDetailModels", new[] { "CategoryId" });
            DropIndex("dbo.TransactionDetailModels", new[] { "PayeeId" });
            DropIndex("dbo.TransactionDetailModels", new[] { "TransactionId" });
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
            DropTable("dbo.ImportNameToPayeeModels");
            DropTable("dbo.PayeeDefaultDetailsModels");
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
