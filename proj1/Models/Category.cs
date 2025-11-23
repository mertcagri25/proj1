using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc; // Required for [Remote] attribute

namespace proj1.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [Remote(action: "VerifyName", controller: "Category", AdditionalFields = "Id")]
        public required string Name { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<News>? News { get; set; }
    }
}
