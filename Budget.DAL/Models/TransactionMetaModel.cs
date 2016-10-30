using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class TransactionMetaModel
    {
        [Key]
        [Column(Order = 1)]
        public int TransactionId { get; set; }

        public int CheckNum { get; set; }

        [Required]
        public TransactionStatus Status { get; set; }

        public int TopPayeeId { get; set; }

        public string TopMemo { get; set; }

        [Required]
        public DateTime LastEditDate { get; set; }
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
