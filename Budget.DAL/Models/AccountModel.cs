using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Budget.DAL.Models
{
    public class AccountModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("FinancialInstitution")]
        [Required]
        [Index("IX_UserFinancialInstitutionAccountNumber", 2, IsUnique = true)]
        [Display(Name = "Financial Institution ID")]
        public int FinancialInstitutionId { get; set; }

        [Required]
        [Index("IX_UserFinancialInstitutionAccountNumber", 3, IsUnique = true)]
        [StringLength(100)]
        [Display(Name = "Account Number")]
        public string Number { get; set; }

        [Display(Name = "OFX BANKID field, Bank routing number or other FI ID")]
        public int RoutingNumber { get; set; }

        [StringLength(150)]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Account Type")]
        public AccountType Type { get; set; }

        [Required]
        [StringLength(400)]
        [Display(Name = "Account Description")]
        public string Description { get; set; }

        public ICollection<TransactionModel> Transactions { get; set; }

        public ICollection<BalanceModel> BalanceHistory { get; set; }

        [JsonIgnore]
        public virtual FinancialInstitutionModel FinancialInstitution { get; set; }

        public AccountModel()
        {
            Transactions = new List<TransactionModel>();
            BalanceHistory = new List<BalanceModel>();
            RoutingNumber = 0;
            Name = string.Empty;
        }
    }

    public enum AccountType
    {
        Savings,
        Checking,
        CreditCard
    }

}