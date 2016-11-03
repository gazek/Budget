using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class PayeeModel
    {
        [Key]
        [Display(Name = "Payee ID")]
        public int Id { get; set; }

        [Required]
        [Index("IX_NameUserId", 1, IsUnique = true)]
        [Display(Name = "Payee Name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Index("IX_NameUserId", 2, IsUnique = true)]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        public ICollection<PayeeDefaultDetails> DefaultDetails { get; set; }

        public ICollection<ImportNameToPayeeModel> ImportNames { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
    }
}
