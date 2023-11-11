using System.ComponentModel.DataAnnotations;

namespace FileSharing.Shared.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Incorrect email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        [Compare("Password", ErrorMessage = "Passwords must be equal")]
        public string PasswordConfirm { get; set; } = string.Empty;
    }
}
