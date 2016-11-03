using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class ImportNameToPayeeModel
    {
        [Key]
        [Column(Order = 1)]
        [Required]
        public int PayeeId { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [StringLength(100)]
        public string ImportName { get; set; }

        [ForeignKey("PayeeId")]
        [JsonIgnore]
        public virtual PayeeModel Payee { get; set; }
    }
}
