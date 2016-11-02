using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    class FinancialInstitutionsUserDetailsModel
    {
        [Key]
        [Column(Order = 1)]
        [Required]
        [Display(Name = "OFX FID Attribute")]
        public FinancialInstitutionModel FinancialInstitutionId { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [ForeignKey("FinancialInstitutionId")]
        public virtual FinancialInstitutionModel FinancialInstitution { get; set; }

        // User override of default value or forced inclusion of OFX Fields
        // null or empty string will cause default value to be used
        public string CLIENTUID { get; set; }
    }
}
