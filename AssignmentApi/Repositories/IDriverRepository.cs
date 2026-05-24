using System.Threading.Tasks;
using AssignmentApi.DTOs;
using AssignmentApi.Models;

namespace AssignmentApi.Repositories
{
    public interface IDriverRepository
    {
        Task<User?> GetUserWithRoleByUsernameAsync(string username);
        Task<bool> SessionValueExistsAsync(string sessionValue);
        Task SaveDriverLoginSessionAsync(User user, string sessionValue);
        Task<ProcedureResponse> ExecuteDriverLocationCleanupAsync(int userId, string locationName);
        Task<User?> GetActiveDriverByIdAsync(int userId);
        Task<Location?> GetActiveLocationByNameAsync(string locationName);
        Task UpdateDriverLocationAsync(User user, Location location);
    }
}
