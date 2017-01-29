using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.API.Models
{
    public class PayeeImportNameViewModel
    {
        public int Id { get; set; }
        public int PayeeId { get; set; }
        public string PayeeName { get; set; }
        public string ImportName { get; set; }
    }
}