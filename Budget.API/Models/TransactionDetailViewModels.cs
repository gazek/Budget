using System;

namespace Budget.API.Models
{
    public class TransactionDetailViewModel
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public int PayeeId { get; set; }
        public string PayeeName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public decimal Amount { get; set; }
        public int? TransferTransactionId { get; set; }
        public string Memo { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}