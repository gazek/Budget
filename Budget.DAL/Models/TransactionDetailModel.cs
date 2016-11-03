using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class TransactionDetailModel
    {
        [Key]
        [Column(Order = 1)]
        public int TransactionId { get; set; }

        [Key]
        [Column(Order = 3)]
        public int CategoryId { get; set; }

        [Key]
        [Column(Order = 4)]
        public int SubCategoryId { get; set; }

        public decimal Amount { get; set; }

        public int TransferTransactionId { get; set; }

        public string Memo { get; set; }

        [Required]
        public DateTime LastEditDate { get; set; }

        [ForeignKey("TransactionId")]
        [JsonIgnore]
        public TransactionModel Transaction { get; set; }
        
        [ForeignKey("CategoryId")]
        [JsonIgnore]
        public CategoryModel Category { get; set; }
        
        [ForeignKey("SubCategoryId")]
        [JsonIgnore]
        public SubCategoryModel SubCategory { get; set; }
        
        [ForeignKey("TransferTransactionId")]
        [JsonIgnore]
        public TransactionModel TransferMatch { get; set; }
    }
}
