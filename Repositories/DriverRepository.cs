using System;
using System.Data;
using System.Threading.Tasks;
using AssignmentApi.Data;
using AssignmentApi.DTOs;
using AssignmentApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssignmentApi.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly AssignmentDbContext _context;

        public DriverRepository(AssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserWithRoleByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> SessionValueExistsAsync(string sessionValue)
        {
            return await _context.Users.AnyAsync(u => u.SessionValue == sessionValue);
        }

        public async Task SaveDriverLoginSessionAsync(User user, string sessionValue)
        {
            user.SessionValue = sessionValue;
            user.UserActiveOnline = "Y";
            user.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<ProcedureResponse> ExecuteDriverLocationCleanupAsync(int userId, string locationName)
        {
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SP_DriverLocation_CleanUP";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@UserId", userId));
            command.Parameters.Add(new SqlParameter("@LocationName", locationName));
            var returnResponseParameter = new SqlParameter("@ReturnResponse", SqlDbType.VarChar, 400)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(returnResponseParameter);

            if (command.Connection != null && command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync();
            }

            try
            {
                await command.ExecuteNonQueryAsync();
                var returnResponse = returnResponseParameter.Value == DBNull.Value
                    ? string.Empty
                    : returnResponseParameter.Value?.ToString() ?? string.Empty;

                return new ProcedureResponse
                {
                    Status = IsSuccessResponse(returnResponse),
                    Message = returnResponse
                };
            }
            catch (SqlException ex)
            {
                return new ProcedureResponse
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<User?> GetActiveDriverByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId
                    && u.UserActiveOnline == "Y"
                    && u.Role != null
                    && u.Role.Active == "Y"
                    && u.Role.RoleName == "Driver");
        }

        public async Task<Location?> GetActiveLocationByNameAsync(string locationName)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(l => l.LocationName == locationName && l.Active == "Y");
        }

        public async Task UpdateDriverLocationAsync(User user, Location location)
        {
            user.LocationId = location.LocationId;
            user.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static bool IsSuccessResponse(string returnResponse)
        {
            return returnResponse.Contains("success", StringComparison.OrdinalIgnoreCase);
        }
    }
}
