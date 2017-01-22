using Budget.DAL.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Budget.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public DbSet<FinancialInstitutionModel> FinancialInstitutions { get; set; }
        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<BalanceModel> Balances { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<TransactionDetailModel> TransactionDetails { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<SubCategoryModel> SubCategories { get; set; }
        public DbSet<PayeeModel> Payees { get; set; }
        public DbSet<ImportNameToPayeeModel> ImportNames { get; set; }
        public DbSet<PayeeDefaultDetailsModel> DefaultDetails { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountModel>()
                .HasRequired<FinancialInstitutionModel>(a => a.FinancialInstitution)
                .WithMany(f => f.Accounts)
                .HasForeignKey(a => a.FinancialInstitutionId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<BalanceModel>()
                .HasRequired<AccountModel>(b => b.Account)
                .WithMany(a => a.BalanceHistory)
                .HasForeignKey(b => b.AccountId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CategoryModel>()
                .HasRequired<IdentityUser>(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FinancialInstitutionModel>()
                .HasRequired<IdentityUser>(fi => fi.User)
                .WithMany()
                .HasForeignKey(fi => fi.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImportNameToPayeeModel>()
                .HasRequired<PayeeModel>(i => i.Payee)
                .WithMany(p => p.ImportNames)
                .HasForeignKey(i => i.PayeeId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PayeeDefaultDetailsModel>()
                .HasRequired<PayeeModel>(d => d.Payee)
                .WithMany(p => p.DefaultDetails)
                .HasForeignKey(d => d.PayeeId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PayeeModel>()
                .HasRequired<IdentityUser>(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<SubCategoryModel>()
                .HasRequired(s => s.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(s => s.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TransactionDetailModel>()
                .HasRequired(d => d.Transaction)
                .WithMany(t => t.Details)
                .HasForeignKey(d => d.TransactionId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TransactionModel>()
                .HasRequired(d => d.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(d => d.AccountId)
                .WillCascadeOnDelete(true);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
