using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class FinancialInstitutionModel
    {
        [Key]
        [Display(Name = "Financial Institution ID")]
        public int Id { get; set; }

        [Required]
        [Index("IX_FIDFINameUserId", 1, IsUnique = true)]
        [Display(Name = "OFX FID Attribute")]
        public int OfxFid { get; set; }

        [Required]
        [Index("IX_FIDFINameUserId", 2, IsUnique = true)]
        [Display(Name = "Financial Institution Name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "OFX URL")]
        [Url]
        public string OfxUrl { get; set; }

        [Required]
        [Display(Name = "OFX Org Attribute")]
        [StringLength(50)]
        public string OfxOrg { get; set; }

        [Required]
        [Index("IX_FIDFINameUserId", 3, IsUnique = true)]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Financial Institution Login Username")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }

        // User override of default value or forced inclusion of OFX Fields
        // null or empty string will cause default value to be used
        public string CLIENTUID { get; set; }
    }
}
