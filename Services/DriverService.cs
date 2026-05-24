using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AssignmentApi.DTOs;
using AssignmentApi.Models;
using AssignmentApi.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AssignmentApi.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        public DriverService(IDriverRepository driverRepository, IConfiguration configuration)
        {
            _driverRepository = driverRepository;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<ApiResponse<DriverLoginData>> LoginAsync(DriverLoginRequest request)
        {
            var user = await _driverRepository.GetUserWithRoleByUsernameAsync(request.Username);
            if (user == null)
            {
                return new ApiResponse<DriverLoginData>
                {
                    Status = false,
                    Message = "User is not present in system"
                };
            }

            if (user.Role == null || user.Role.Active != "Y" || user.Role.RoleName != "Driver")
            {
                return new ApiResponse<DriverLoginData>
                {
                    Status = false,
                    Message = "User is not an active Driver"
                };
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return new ApiResponse<DriverLoginData>
                {
                    Status = false,
                    Message = "Invalid username or password"
                };
            }

            var sessionValue = await GenerateUniqueSessionValueAsync();
            await _driverRepository.SaveDriverLoginSessionAsync(user, sessionValue);
            var token = GenerateJwtToken(user, user.Role.RoleName, sessionValue);

            return new ApiResponse<DriverLoginData>
            {
                Status = true,
                Message = "Login successful",
                Data = new DriverLoginData
                {
                    UserId = user.UserId,
                    Role = user.Role.RoleName,
                    Token = token,
                    SessionValue = sessionValue
                }
            };
        }

        public async Task<ProcedureResponse> UpdateCurrentLocationAsync(DriverLocationUpdateRequest request)
        {
            return await _driverRepository.ExecuteDriverLocationCleanupAsync(request.UserId, request.LocationName);
        }

        private async Task<string> GenerateUniqueSessionValueAsync()
        {
            string sessionValue;
            do
            {
                sessionValue = Guid.NewGuid().ToString();
            }
            while (await _driverRepository.SessionValueExistsAsync(sessionValue));

            return sessionValue;
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
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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
