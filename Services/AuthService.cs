using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AssignmentApi.Data;
using AssignmentApi.DTOs;
using AssignmentApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AssignmentApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AssignmentDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(AssignmentDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<(bool Success, string Message, int? UserId)> RegisterAsync(RegisterRequest request)
        {
            var requestedRole = request.Role.Trim();
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Active == "Y"
                    && (r.RoleName == requestedRole || r.RoleCode == requestedRole));

            if (role == null)
            {
                return (false, $"Role '{requestedRole}' is not available in active Role master.", null);
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Active == "Y");

            if (location == null)
            {
                return (false, "Active location does not exist.", null);
            }

            var alreadyExists = await _context.Users.AnyAsync(u =>
                u.Username == request.Username || u.UserEmail == request.UserEmail || u.UserPhoneNo == request.UserPhoneNo);

            if (alreadyExists)
            {
                return (false, "Username, email, or phone number already exists.", null);
            }

            var user = new User
            {
                Username = request.Username,
                RoleId = role.RoleId,
                LocationId = location.LocationId,
                UserEmail = request.UserEmail,
                UserPhoneNo = request.UserPhoneNo,
                UserActiveOnline = "N",
                CreatedDate = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.UserPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Registration successful.", user.UserId);
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || user.Role == null || user.Role.Active != "Y")
            {
                return null;
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.UserPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var sessionValue = Guid.NewGuid().ToString();
            user.SessionValue = sessionValue;
            user.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Token = GenerateJwtToken(user, user.Role.RoleName, sessionValue),
                SessionValue = sessionValue,
                UserId = user.UserId,
                RoleName = user.Role.RoleName
            };
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.SessionValue = null;
            user.UserActiveOnline = "N";
            user.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int?> ValidateSessionAsync(string sessionValue)
        {
            if (string.IsNullOrWhiteSpace(sessionValue))
            {
                return null;
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.SessionValue == sessionValue);

            return user?.UserId;
        }

        private string GenerateJwtToken(User user, string roleName, string sessionValue)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "SuperSecretKeyForRideMatchingSystemTokenGeneration12345");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.UserEmail),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim("SessionValue", sessionValue)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
