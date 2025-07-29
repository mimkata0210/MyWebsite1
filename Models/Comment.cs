using System;
using System.ComponentModel.DataAnnotations;

namespace MyWebsite1.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string UserId { get; set; }  // Foreign Key към Identity User
        public ApplicationUser User { get; set; }

        public int PhotoId { get; set; }  // Foreign Key към Photo
        public Photo Photo { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
