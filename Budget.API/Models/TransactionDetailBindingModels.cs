using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budget.API.Models
{
    public class TransactionDetailBindingModel
    {
        [Required]
        public int PayeeId { get; set; }

        [Required]
        public int CategoryId { get; set; }
        
        public int SubCategoryId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public int? TransferTransactionId { get; set; }

        [Required]
        public string Memo { get; set; }
    }
}