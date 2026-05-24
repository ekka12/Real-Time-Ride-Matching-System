using System.Threading.Tasks;
using AssignmentApi.DTOs;

namespace AssignmentApi.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, int? UserId)> RegisterAsync(RegisterRequest request);
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<bool> LogoutAsync(int userId);
        Task<int?> ValidateSessionAsync(string sessionValue);
    }
}
