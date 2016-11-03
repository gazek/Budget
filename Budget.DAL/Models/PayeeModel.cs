using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class PayeeModel
    {
        [Key]
        [Required]
        [Index(IsUnique = true)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Required]
        public IEnumerable<PayeeDefaultDetails> DefaultDetails { get; set; }

        [Required]
        public IEnumerable<ImportNameToPayeeModel> ImportNames { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
    }
}
