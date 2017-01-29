using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class PayeeImportNameModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Payee")]
        [Required]
        [Index("IX_PayeeImportName", 1, IsUnique = true)]
        public int PayeeId { get; set; }

        [Required]
        [Index("IX_PayeeImportName", 2, IsUnique = true)]
        [StringLength(100)]
        public string ImportName { get; set; }

        [JsonIgnore]
        public virtual PayeeModel Payee { get; set; }
    }
}
