using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budget.API.Models
{
    public class PayeeBindingModel
    {
        [Required]
        public string Name { get; set; }
    }
}