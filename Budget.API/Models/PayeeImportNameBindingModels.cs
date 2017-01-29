using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.API.Models
{
    public class PayeeImportNameBindingModel
    {
        public int PayeeId { get; set; }
        public string ImportName { get; set; }
    }
}