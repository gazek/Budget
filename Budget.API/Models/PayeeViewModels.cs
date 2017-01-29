using Budget.DAL.Models;
using System.Collections.Generic;

namespace Budget.API.Models
{
    public class PayeeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<PayeeDefaultDetailViewModel> DefaultDetails { get; set; }
        public ICollection<PayeeImportNameViewModel> ImportNames { get; set; }
    }
}