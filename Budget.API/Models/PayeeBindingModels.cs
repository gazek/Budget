using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Budget.API.Models
{
    public class PayeeBindingModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<PayeeDefaultDetailBindingModel> DefaultDetails { get; set; }
        public ICollection<PayeeImportNameBindingModel> ImportNames { get; set; }
    }
}