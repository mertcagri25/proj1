using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace proj1.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<News>? News { get; set; }
    }
}
