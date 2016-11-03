using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Budget.DAL.Models
{
    public class AccountModel
    {
        [Key]
        [Required]
        [Index(IsUnique = true)]
        [Display(Name = "Account ID")]
        public int Id { get; set; }

        // App ID of account owner
        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Financial Institution ID")]
        public int FinancialInstitutionId { get; set; }

        [Required]
        [Display(Name = "Account Number")]
        public string Number { get; set; }

        [Required]
        [Display(Name = "Account Type")]
        public AccountType Type { get; set; }

        [Required]
        [Display(Name = "Account Description")]
        public string Description { get; set; }

        public IEnumerable<TransactionModel> Transactions { get; set; }

        public IEnumerable<BalanceModel> BalanceHistory { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual IdentityUser User { get; set; }

        [ForeignKey("FinancialInstitutionId")]
        [JsonIgnore]
        public virtual FinancialInstitutionModel FinancialInstitution { get; set; }
    }

    public enum AccountType
    {
        Savings,
        Checking,
        CreditCard
    }

}