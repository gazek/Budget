using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class TransactionDetailModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Transaction")]
        [Required]
        [Index("IX_TransDetailTransPayeeCatSubUnique", 1, IsUnique = true)]
        public int TransactionId { get; set; }

        [ForeignKey("Payee")]
        [Required]
        [Index("IX_TransDetailTransPayeeCatSubUnique", 2, IsUnique = true)]
        public int PayeeId { get; set; }

        [ForeignKey("Category")]
        [Required]
        [Index("IX_TransDetailTransPayeeCatSubUnique", 3, IsUnique = true)]
        public int CategoryId { get; set; }

        [ForeignKey("SubCategory")]
        [Required]
        [Index("IX_TransDetailTransPayeeCatSubUnique", 4, IsUnique = true)]
        public int SubCategoryId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [ForeignKey("TransferMatch")]
        public int? TransferMatchId { get; set; }

        [StringLength(400)]
        public string Memo { get; set; }

        [Required]
        public DateTime LastEditDate { get; set; }

        [JsonIgnore]
        public virtual TransactionModel Transaction { get; set; }

        [JsonIgnore]
        public virtual PayeeModel Payee { get; set; }

        [JsonIgnore]
        public virtual CategoryModel Category { get; set; }
        
        [JsonIgnore]
        public virtual SubCategoryModel SubCategory { get; set; }
        
        [JsonIgnore]
        public virtual TransactionModel TransferMatch { get; set; }

        public TransactionDetailModel()
        {
            Memo = string.Empty;
            LastEditDate = DateTime.Now;
        }
    }
}
