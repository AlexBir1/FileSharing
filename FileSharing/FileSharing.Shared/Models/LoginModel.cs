﻿using System.ComponentModel.DataAnnotations;

namespace FileSharing.Shared.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username or Email is required")]
        public string someid { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
