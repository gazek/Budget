using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Budget.DAL.Models
{
    public class PayeeModel
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        public IEnumerable<PayeeDefaultDetails> DefaultDetails { get; set; }

        public IEnumerable<ImportNameToPayeeModel> ImportNames { get; set; }

    }
}
