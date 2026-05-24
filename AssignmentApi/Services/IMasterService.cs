using System.Collections.Generic;
using System.Threading.Tasks;
using AssignmentApi.DTOs;
using AssignmentApi.Models;

namespace AssignmentApi.Services
{
    public interface IMasterService
    {
        Task<Location> AddLocationAsync(AddLocationRequest request);
        Task<Role> AddRoleAsync(AddRoleRequest request);
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<bool> DeleteUserAsync(int id);
    }
}
