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

        [Required]
        [Index("IX_TransDetailTransPayeeCatSubUnique", 1, IsUnique = true)]
        public int TransactionId { get; set; }

        [Required]
        [Index("IX_TransDetailTransPayeeCatSubUnique", 2, IsUnique = true)]
        public int PayeeId { get; set; }

        [Required]
        [Index("IX_TransDetailTransPayeeCatSubUnique", 3, IsUnique = true)]
        public int CategoryId { get; set; }

        [Required]
        [Index("IX_TransDetailTransPayeeCatSubUnique", 4, IsUnique = true)]
        public int SubCategoryId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public int? TransferTransactionId { get; set; }

        [StringLength(400)]
        public string Memo { get; set; }

        [Required]
        public DateTime LastEditDate { get; set; }

        [ForeignKey("TransactionId")]
        [JsonIgnore]
        public virtual TransactionModel Transaction { get; set; }

        [ForeignKey("PayeeId")]
        [JsonIgnore]
        public virtual PayeeModel Payee { get; set; }

        [ForeignKey("CategoryId")]
        [JsonIgnore]
        public virtual CategoryModel Category { get; set; }
        
        [ForeignKey("SubCategoryId")]
        [JsonIgnore]
        public virtual SubCategoryModel SubCategory { get; set; }
        
        [ForeignKey("TransferTransactionId")]
        [JsonIgnore]
        public virtual TransactionModel TransferMatch { get; set; }

        public TransactionDetailModel()
        {
            Memo = "";
            LastEditDate = DateTime.Now;
        }
    }
}
