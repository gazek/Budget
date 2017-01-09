using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.API.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<SubCategoryViewModel> SubCategories { get; set; }
    }
}