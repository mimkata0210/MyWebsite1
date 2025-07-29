using System;
using System.ComponentModel.DataAnnotations;

namespace MyWebsite1.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // Foreign Key to Identity user
        public ApplicationUser User { get; set; }
        public string Title { get; set; }
        public byte[] ImageData { get; set; }  // Store image as byte array
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

}
