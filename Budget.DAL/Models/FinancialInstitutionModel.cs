using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Budget.DAL.Models
{
    public class FinancialInstitutionModel
    {
        [Required]
        [Display(Name = "Financial Institution ID")]
        public int Id { get; set; }

        [Key]
        [Required]
        [Display(Name = "OFX ID")]
        public int OfxId { get; set; }

        [Required]
        [Display(Name = "Financial Institution Name")]
        public string Name { get; set; }

        [Key]
        [Required]
        [Display(Name = "OFX URL")]
        [Url]
        public string OfxUrl { get; set; }

        [Required]
        [Display(Name = "OFX Org Attribute")]
        public string Org { get; set; }
    }
}
