using Budget.DAL.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Budget.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<BalanceModel> Balances { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<FinancialInstitutionModel> FinancialInstitutions { get; set; }
        public DbSet<PayeeModel> Payees { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountModel>()
                .HasRequired<IdentityUser>(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CategoryModel>()
                .HasRequired<IdentityUser>(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FinancialInstitutionModel>()
                .HasRequired<IdentityUser>(fi => fi.User)
                .WithMany()
                .HasForeignKey(fi => fi.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PayeeModel>()
                .HasRequired<IdentityUser>(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(false);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
