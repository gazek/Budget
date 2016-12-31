namespace Budget.DAL.Migrations
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<Budget.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Budget.DAL.ApplicationDbContext context)
        {
            // Add user
            context.Users.AddOrUpdate(
                new ApplicationUser()
                {
                    Id = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    Email = "greg@mail.com",
                    PasswordHash = "ABl6i0uHUzgyhc9U6FdYMut+yNyYx22JM3Oz+qPaAfu2bycKYaH5xlzKnYcwrUmTfQ==",
                    SecurityStamp = "89d4cfb7-b5ed-434e-8cea-324e411899fa",
                    UserName = "greg"
                });
            context.SaveChanges();
            
            // Add Financial Institutions
            context.FinancialInstitutions.AddOrUpdate(
                new FinancialInstitutionModel()
                {
                    Id = 1,
                    OfxFid = 3169,
                    Name = "First Tech",
                    OfxUrl = "https://ofx.firsttechfed.com",
                    OfxOrg = "First Tech Federal Credit Union",
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    Username = "gwaliczek",
                    PasswordHash = ParseHex("0xEC060FDDFD0B223235D753D82874C24B")
                },
                new FinancialInstitutionModel()
                {
                    Id = 1,
                    OfxFid = 10898,
                    Name = "Chase Visa",
                    OfxUrl = "https://ofx.chase.com",
                    OfxOrg = "B1",
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    Username = "lwali442",
                    PasswordHash = ParseHex("0xC2C6DBBDB069D0293B368BFBEED4BEBB"),
                    CLIENTUID = "94f92863-15c1-4874-9fe5-0c84351ac0c2"
                });
            context.SaveChanges();
            
            // Add accounts
            context.Accounts.AddOrUpdate(
                new AccountModel()
                {
                    Id = 1,
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    FinancialInstitutionId = 1,
                    Number = "S9321268709",
                    RoutingNumber = 321180379,
                    Name = "Carefree Checking",
                    Type = AccountType.Checking,
                    Description = "Carefree Checking"
                },
                new AccountModel()
                {
                    Id = 2,
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    FinancialInstitutionId = 1,
                    Number = "S9321033798",
                    RoutingNumber = 321180379,
                    Name = "Long Term Savings",
                    Type = AccountType.Savings,
                    Description = "Long Term Savings"
                },
                new AccountModel()
                {
                    Id = 3,
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    FinancialInstitutionId = 1,
                    Number = "S9322856239",
                    RoutingNumber = 321180379,
                    Name = "Travel Savings",
                    Type = AccountType.Savings,
                    Description = "Travel Savings"
                },
                new AccountModel()
                {
                    Id = 4,
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    FinancialInstitutionId = 1,
                    Number = "S9322856221",
                    RoutingNumber = 321180379,
                    Name = "Debt Paydown",
                    Type = AccountType.Savings,
                    Description = "Debt Paydown"
                },
                new AccountModel()
                {
                    Id = 5,
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    FinancialInstitutionId = 2,
                    Number = "4388576083912694",
                    Name = "Chase Visa",
                    Type = AccountType.CreditCard,
                    Description = "Chase Visa"
                });
            context.SaveChanges();
            
            // add categories
            context.Categories.AddOrUpdate(
                new CategoryModel()
                {
                    Id = 1,
                    Name = "Food",
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    SubCategories = new List<SubCategoryModel>()
                    {
                        new SubCategoryModel()
                        {
                            Id = 1,
                            CategoryId = 1,
                            Name = "Groceries"
                        },
                        new SubCategoryModel()
                        {
                            Id = 2,
                            CategoryId = 1,
                            Name = "Take out"
                        }
                    }
                },
                new CategoryModel()
                {
                    Id = 2,
                    Name = "Income",
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86"
                });
            context.SaveChanges();
            
            // Add Payees
            context.Payees.AddOrUpdate(
                new PayeeModel()
                {
                    Id = 1,
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    Name = "Whole Foods",
                    ImportNames = new List<ImportNameToPayeeModel>()
                    {
                        new ImportNameToPayeeModel()
                        {
                            Id = 1,
                            PayeeId = 1,
                            ImportName = "WHOLEFDS"
                        }
                    },
                    DefaultDetails = new List<PayeeDefaultDetailsModel>()
                    {
                        new PayeeDefaultDetailsModel()
                        {
                            Id = 1,
                            CategoryId = 1,
                            SubCategoryId = 1,
                            Allocation = 1.00m,
                            PayeeId = 1
                        }
                    }
                },
                new PayeeModel()
                {
                    Id = 2,
                    UserId = "ababad5d-ee1a-4fe3-b150-cb5f8c5a5a86",
                    Name = "New Seasons",
                    ImportNames = new List<ImportNameToPayeeModel>()
                    {
                        new ImportNameToPayeeModel()
                        {
                            Id = 2,
                            PayeeId = 2,
                            ImportName = "NEW SEASONS MARKET"
                        }
                    },
                    DefaultDetails = new List<PayeeDefaultDetailsModel>()
                    {
                        new PayeeDefaultDetailsModel()
                        {
                            Id = 2,
                            CategoryId = 1,
                            SubCategoryId = 1,
                            Allocation = 0.25m,
                            PayeeId = 2
                        },
                        new PayeeDefaultDetailsModel()
                        {
                            Id = 2,
                            CategoryId = 1,
                            SubCategoryId = 2,
                            Allocation = 0.75m,
                            PayeeId = 2
                        }
                    }
                });
            context.SaveChanges();
        }

        static byte[] ParseHex(string hex)
        {
            int offset = hex.StartsWith("0x") ? 2 : 0;
            if ((hex.Length % 2) != 0)
            {
                throw new ArgumentException("Invalid length: " + hex.Length);
            }
            byte[] ret = new byte[(hex.Length - offset) / 2];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte)((ParseNybble(hex[offset]) << 4)
                                 | ParseNybble(hex[offset + 1]));
                offset += 2;
            }
            return ret;
        }

        static int ParseNybble(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            if (c >= 'A' && c <= 'F')
            {
                return c - 'A' + 10;
            }
            if (c >= 'a' && c <= 'f')
            {
                return c - 'a' + 10;
            }
            throw new ArgumentException("Invalid hex digit: " + c);
        }
    }
}
