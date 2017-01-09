using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.API.Models
{
    public class SubCategoryViewModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}