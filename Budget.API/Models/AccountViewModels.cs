using Budget.DAL.Models;

namespace Budget.API.Models
{
    public class AccountListViewModel
    {
        public int FinancialInstitutionId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public AccountType Type { get; set; }
    }
}