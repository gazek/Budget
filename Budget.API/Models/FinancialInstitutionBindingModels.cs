using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.API.Models
{
    public class FinancialInstitutionCreateBindingModel
    {
        [Display(Name = "Financial Institution ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name of financial institution")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "OFX FID Attribute")]
        public int OfxFid { get; set; }

        [Required]
        [Display(Name = "OFX URL")]
        [Url]
        public string OfxUrl { get; set; }

        [Required]
        [Display(Name = "OFX Org Attribute")]
        [StringLength(50)]
        public string OfxOrg { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // User override of default value or forced inclusion of OFX Fields
        // null or empty string will cause default value to be used
        [Display(Name = "OFX CLIENTUID field")]
        public string CLIENTUID { get; set; }
    }

    public class FinancialInstitutionUpdateBindingModel
    {
        [Display(Name = "Financial Institution ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name of financial institution")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "OFX FID Attribute")]
        public int OfxFid { get; set; }

        [Required]
        [Display(Name = "OFX URL")]
        [Url]
        public string OfxUrl { get; set; }

        [Required]
        [Display(Name = "OFX Org Attribute")]
        [StringLength(50)]
        public string OfxOrg { get; set; }

        // User override of default value or forced inclusion of OFX Fields
        // null or empty string will cause default value to be used
        [Display(Name = "OFX CLIENTUID field")]
        public string CLIENTUID { get; set; }
    }

    public class FinancialInstitutionUpdateLoginBindingModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
