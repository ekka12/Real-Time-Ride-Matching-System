using System.ComponentModel.DataAnnotations;

namespace AssignmentApi.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^(?=.{8,15}$)(?=\S+$)(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$",
            ErrorMessage = "Password must be 8-15 characters, contain no spaces, and include at least one uppercase letter, one number, and one special character.")]
        public string UserPassword { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^[^@\s]+@[^@\s]+\.com$",
            ErrorMessage = "Email must contain '@' in the middle and end with '.com'.")]
        [StringLength(150)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string UserPhoneNo { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string UserPassword { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string SessionValue { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
