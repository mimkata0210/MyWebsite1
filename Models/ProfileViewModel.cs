using System.ComponentModel.DataAnnotations;

namespace MyWebsite1.Models
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Потребителско име")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // Полета за смяна на парола
        [DataType(DataType.Password)]
        [Display(Name = "Стара парола")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Нова парола")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Потвърди новата парола")]
        [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmNewPassword { get; set; }
    }
}
