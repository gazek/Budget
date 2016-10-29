using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Budget.DAL.Models
{
    public class AccountModel
    {
        [Required]
        [Display(Name = "Account ID")]
        public int Id { get; set; }

        // ID of account owner
        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        [Display(Name = "Financial Institution ID")]
        public int FiId { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [Display(Name = "Account Number")]
        public string Number { get; set; }

        [Required]
        [Display(Name = "Account Type")]
        public AccountType Type { get; set; }

        [Required]
        [Display(Name = "Account Description")]
        public string Description { get; set; }

        [Required]
        public IEnumerable<TransactionModel> Transactions { get; set; }

        // needed for foreign key
        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual IdentityUser Owner { get; set; }

        // needed for foreign key
        [ForeignKey("FiId")]
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