using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class ImportNameToPayeeModel
    {
        [Key]
        [Column(Order = 1)]
        public int Id { get; set; }

        [Index("IX_PayeeImportName", 1, IsUnique = true)]
        [Required]
        public int PayeeId { get; set; }

        [Index("IX_PayeeImportName", 2, IsUnique = true)]
        [Required]
        [StringLength(100)]
        public string ImportName { get; set; }

        [ForeignKey("PayeeId")]
        [JsonIgnore]
        public virtual PayeeModel Payee { get; set; }
    }
}
