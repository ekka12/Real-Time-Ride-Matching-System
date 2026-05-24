using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssignmentApi.Data;
using AssignmentApi.DTOs;
using AssignmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AssignmentApi.Services
{
    public class MasterService : IMasterService
    {
        private readonly AssignmentDbContext _context;

        public MasterService(AssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Location> AddLocationAsync(AddLocationRequest request)
        {
            var location = new Location
            {
                LocationCode = request.LocationCode,
                LocationName = request.LocationName,
                Longitude = (decimal)request.Longitude,
                Latitude = (decimal)request.Latitude,
                Active = request.Active,
                CreatedDate = DateTime.UtcNow
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<Role> AddRoleAsync(AddRoleRequest request)
        {
            var role = new Role
            {
                RoleCode = request.RoleCode,
                RoleName = request.RoleName,
                Active = request.Active,
                CreatedDate = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _context.Locations.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Location)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Location)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Location)
                .Where(u => u.Role != null && u.Role.RoleName == role)
                .ToListAsync();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
