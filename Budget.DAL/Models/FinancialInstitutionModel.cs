using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class FinancialInstitutionModel
    {
        [Key]
        [Index(IsUnique = true)]
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "OFX FID Attribute")]
        public int OfxFid { get; set; }

        [Required]
        [Display(Name = "Financial Institution Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "OFX URL")]
        [Url]
        public string OfxUrl { get; set; }

        [Required]
        [Display(Name = "OFX Org Attribute")]
        public string OfxOrg { get; set; }

        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Financial Institution Login Username")]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual IdentityUser User { get; set; }

        // User override of default value or forced inclusion of OFX Fields
        // null or empty string will cause default value to be used
        public string CLIENTUID { get; set; }
    }
}
