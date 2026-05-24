using System.ComponentModel.DataAnnotations;

namespace AssignmentApi.DTOs
{
    public class RiderLoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RiderLoginData
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string SessionValue { get; set; } = string.Empty;
    }

    public class RiderRideRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string PickUPLocationName { get; set; } = string.Empty;

        [Required]
        public string DropLocationName { get; set; } = string.Empty;
    }
}
