using System;
using System.ComponentModel.DataAnnotations;
using proj1.Constants;

namespace proj1.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = Roles.User; // 'Admin' or 'Editor'

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
