using Budget.DAL.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Budget.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
    {
        public DbSet<AccountModel> Accounts { get; set; }
        //DbSet<BalanceModel> Balances { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<FinancialInstitutionModel> FinancialInstitutions { get; set; }
        public DbSet<PayeeModel> Payees { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
