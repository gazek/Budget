﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class SubCategoryModel
    {
        [Key]
        [Display(Name = "Subcategory ID")]
        public int Id { get; set; }

        [Required]
        [Index("IX_CategoryIdName", 1, IsUnique = true)]
        public int CategoryId { get; set; }

        [Required]
        [Index("IX_CategoryIdName", 2, IsUnique = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [ForeignKey("CategoryId")]
        [JsonIgnore]
        public virtual CategoryModel Category { get; set; }
    }
}
