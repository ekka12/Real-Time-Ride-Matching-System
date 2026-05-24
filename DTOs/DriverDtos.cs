using System.ComponentModel.DataAnnotations;

namespace AssignmentApi.DTOs
{
    public class ApiResponse<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class DriverLoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class DriverLoginData
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string SessionValue { get; set; } = string.Empty;
    }

    public class DriverLocationUpdateRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string LocationName { get; set; } = string.Empty;
    }

    public class ProcedureResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
