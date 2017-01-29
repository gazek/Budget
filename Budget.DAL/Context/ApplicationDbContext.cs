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
        public DbSet<PayeeDefaultDetailModel> PayeeDefaultDetails { get; set; }
        public DbSet<PayeeImportNameModel> PayeeImportNames { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FinancialInstitutionModel>()
                .HasRequired<IdentityUser>(fi => fi.User)
                .WithMany()
                .HasForeignKey(fi => fi.UserId)
                .WillCascadeOnDelete(true);

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

            modelBuilder.Entity<TransactionModel>()
                .HasRequired<AccountModel>(d => d.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(d => d.AccountId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TransactionDetailModel>()
                .HasRequired(d => d.Transaction)
                .WithMany(t => t.Details)
                .HasForeignKey(d => d.TransactionId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TransactionDetailModel>()
                .HasRequired(d => d.Payee)
                .WithMany()
                .HasForeignKey(d => d.PayeeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TransactionDetailModel>()
                .HasRequired(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TransactionDetailModel>()
                .HasRequired(d => d.SubCategory)
                .WithMany()
                .HasForeignKey(d => d.SubCategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CategoryModel>()
                .HasRequired<IdentityUser>(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<SubCategoryModel>()
                .HasRequired(s => s.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(s => s.CategoryId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PayeeModel>()
                .HasRequired<IdentityUser>(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PayeeImportNameModel>()
                .HasRequired<PayeeModel>(i => i.Payee)
                .WithMany(p => p.ImportNames)
                .HasForeignKey(i => i.PayeeId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PayeeDefaultDetailModel>()
                .HasRequired<PayeeModel>(d => d.Payee)
                .WithMany(p => p.DefaultDetails)
                .HasForeignKey(d => d.PayeeId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PayeeDefaultDetailModel>()
                .HasRequired<CategoryModel>(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PayeeDefaultDetailModel>()
               .HasRequired<SubCategoryModel>(d => d.SubCategory)
               .WithMany()
               .HasForeignKey(d => d.SubCategoryId)
               .WillCascadeOnDelete(false);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
