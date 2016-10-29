using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class FinancialInstitutionModel
    {
        [Key]
        [Column(Order = 1)]
        [Required]
        [Display(Name = "OFX FID Attribute")]
        public int OfxFid { get; set; }

        [Required]
        [Display(Name = "Financial Institution Name")]
        public string Name { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [Display(Name = "OFX URL")]
        [Url]
        public string OfxUrl { get; set; }

        [Required]
        [Display(Name = "OFX Org Attribute")]
        public string Org { get; set; }
    }
}
