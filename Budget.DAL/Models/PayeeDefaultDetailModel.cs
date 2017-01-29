using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class PayeeDefaultDetailModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Payee")]
        [Required]
        [Index("IX_PayeeCatSubCatIds", 1, IsUnique = true)]
        public int PayeeId { get; set; }

        [ForeignKey("Category")]
        [Required]
        [Index("IX_PayeeCatSubCatIds", 2, IsUnique = true)]
        public int CategoryId { get; set; }

        [ForeignKey("SubCategory")]
        [Required]
        [Index("IX_PayeeCatSubCatIds", 3, IsUnique = true)]
        public int SubCategoryId { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal Allocation { get; set; }

        [JsonIgnore]
        public virtual PayeeModel Payee { get; set; }

        [JsonIgnore]
        public virtual CategoryModel Category { get; set; }

        [JsonIgnore]
        public virtual SubCategoryModel SubCategory { get; set; }
    }
}
