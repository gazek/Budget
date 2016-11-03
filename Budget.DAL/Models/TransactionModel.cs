using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class TransactionModel
    {
        [Key]
        [Required]
        [Index(IsUnique = true)]
        public int Id { get; set; }

        [Index("IX_AccountIdReferenceValueAndDate", 1, IsUnique = true)]
        [Required]
        public int AccountId { get; set; }

        [Index("IX_AccountIdReferenceValueAndDate", 2, IsUnique = true)]
        [Required]
        // Finncial institution provided transaction reference value/identifier
        public int ReferenceValue { get; set; }

        [Index("IX_AccountIdReferenceValueAndDate", 3, IsUnique = true)]
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal Amount { get; set; }

        // Finncial institution provided name of payee
        [Required]
        public string OriginalPayeeName { get; set; }

        // Finncial institution provided memo
        [Required]
        public string OriginalMemo { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        [Required]
        public TransactionStatus Status { get; set; }

        public int TopPayeeId { get; set; }

        public string TopMemo { get; set; }

        public int CheckNum { get; set; }

        [Required]
        public DateTime LastEditDate { get; set; }

        [Required]
        public ICollection<TransactionDetailModel> Details { get; set; }

        [ForeignKey("AccountId")]
        [JsonIgnore]
        public AccountModel Account { get; set; }
    }

    public enum TransactionStatus
    {
        New, // newly imported and not reviewed or not fully reviewed
        Attention,  // Partialy or fully reviewed and deemed to have non-routine Acceptance path or otherwise requiring further attention
        Accepted,  // Reviewed and promoted to Accepted register
        Rejected,  // Reviewed but not wanted in Accepted register, will stay in New register and be hidden unless user unhides rejects
        Void // Accepted transaction, lives in Accepted register but does not affect balance, budget or other financial metrics
    }
}
