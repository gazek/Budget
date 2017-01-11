using Budget.API.Models;
using Budget.DAL.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Budget.API.Services
{
    public class TransactionBindingModel
    {
        [Required]
        public TransactionStatus Status { get; set; }

        public int CheckNum { get; set; }
    }
}