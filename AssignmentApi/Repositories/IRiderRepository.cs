using System.Threading.Tasks;
using AssignmentApi.DTOs;
using AssignmentApi.Models;

namespace AssignmentApi.Repositories
{
    public interface IRiderRepository
    {
        Task<User?> GetUserWithRoleByUsernameAsync(string username);
        Task<bool> SessionValueExistsAsync(string sessionValue);
        Task SaveRiderLoginSessionAsync(User user, string sessionValue);
        Task<User?> GetActiveRiderByIdAsync(int userId);
        Task<Location?> GetActiveLocationByNameAsync(string locationName);
        Task<ProcedureResponse> ExecuteRideRequestCleanupAsync(int userId, string pickUpLocationName, string dropLocationName);
    }
}
