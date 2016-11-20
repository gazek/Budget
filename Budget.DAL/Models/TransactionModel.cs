﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class TransactionModel
    {
        [Key]
        public int Id { get; set; }

        [Index("IX_AccountIdReferenceValueAndDate", 1, IsUnique = true)]
        [Required]
        public int AccountId { get; set; }

        [Index("IX_AccountIdReferenceValueAndDate", 2, IsUnique = true)]
        [Required]
        [StringLength(100)]
        // Finncial institution provided transaction reference value/identifier
        public string ReferenceValue { get; set; }

        [Index("IX_AccountIdReferenceValueAndDate", 3, IsUnique = true)]
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal Amount { get; set; }

        // Finncial institution provided name of payee
        [Required]
        [StringLength(200)]
        public string OriginalPayeeName { get; set; }

        // Finncial institution provided memo
        [Required(AllowEmptyStrings=true)]
        [StringLength(200)]
        public string OriginalMemo { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        [Required]
        public TransactionStatus Status { get; set; }

        public int TopPayeeId { get; set; }

        [StringLength(400)]
        public string TopMemo { get; set; }

        public int CheckNum { get; set; }

        [Required]
        public DateTime LastEditDate { get; set; }

        public ICollection<TransactionDetailModel> Details { get; set; }

        [ForeignKey("AccountId")]
        [JsonIgnore]
        public AccountModel Account { get; set; }

        public TransactionModel()
        {
            LastEditDate = DateTime.Today;
            DateAdded = DateTime.Today;
        }
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
