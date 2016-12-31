using Budget.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.API.Models
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string ReferenceValue { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int PayeeId { get; set; }
        public string PayeeName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string Memo { get; set; }
        public string OriginalPayeeName { get; set; }
        public string OriginalMemo { get; set; }
        public DateTime DateAdded { get; set; }
        public TransactionStatus Status { get; set; }
        public int CheckNum { get; set; }
        public DateTime LastEditDate { get; set; }
        public ICollection<TransactionDetailViewModel> Details { get; set; }
    }
}