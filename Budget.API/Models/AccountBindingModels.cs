using Budget.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace Budget.API.Models
{
    public class AccountBindingModel
    {
        [Display(Name = "Account ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Financial Institution ID")]
        public int FinancialInstitutionId { get; set; }

        [Display(Name = "OFX BANKID field, Bank routing number or other FI ID")]
        public int RoutingNumber { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Account Number")]
        public string Number { get; set; }

        [StringLength(150)]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Account Type")]
        public AccountType Type { get; set; }

        [StringLength(400)]
        [Display(Name = "Account Description")]
        public string Description { get; set; }
    }
}