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
        [Column(Order = 2)]
        public int PayeeId { get; set; }

        [Key]
        [Column(Order = 3)]
        public int CategoryId { get; set; }

        [Key]
        [Column(Order = 4)]
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
    }
}
