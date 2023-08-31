using FileSharing.Shared;
using System.ComponentModel.DataAnnotations;

namespace FileSharing.Models
{
    public class AccountCreationModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Incorrect email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select role")]
        public AccountRoles Role { get; set; }
    }
}
