using Budget.DAL.Models;
using System.Data.Entity;

namespace Budget.DAL
{
    internal interface IAppDbContext
    {
        DbSet<AccountModel> Accounts { get; set; }
        //DbSet<BalanceModel> Balances { get; set; }
        DbSet<CategoryModel> Categories { get; set; }
        DbSet<FinancialInstitutionModel> FinancialInstitutions { get; set; }
        DbSet<PayeeModel> Payees { get; set; }
    }
}