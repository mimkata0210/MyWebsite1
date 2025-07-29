using System.ComponentModel.DataAnnotations;

namespace MyWebsite1.Models
{
    public class Download
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public byte[] FileData { get; set; }
        public DateTime DateCreated { get; set; }  // Path to file
        public string FileType { get; set; }  // For example: pdf, exe, zip, etc.
        public string Description { get; set; }
    }
}
