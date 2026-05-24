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
    public class RiderService : IRiderService
    {
        private readonly IRiderRepository _riderRepository;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        public RiderService(IRiderRepository riderRepository, IConfiguration configuration)
        {
            _riderRepository = riderRepository;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<ApiResponse<RiderLoginData>> LoginAsync(RiderLoginRequest request)
        {
            var user = await _riderRepository.GetUserWithRoleByUsernameAsync(request.Username);
            if (user == null)
            {
                return new ApiResponse<RiderLoginData>
                {
                    Status = false,
                    Message = "User is not present in system"
                };
            }

            if (user.Role == null || user.Role.Active != "Y" || user.Role.RoleName != "Rider")
            {
                return new ApiResponse<RiderLoginData>
                {
                    Status = false,
                    Message = "User is not an active Rider"
                };
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return new ApiResponse<RiderLoginData>
                {
                    Status = false,
                    Message = "Invalid username or password"
                };
            }

            var sessionValue = await GenerateUniqueSessionValueAsync();
            await _riderRepository.SaveRiderLoginSessionAsync(user, sessionValue);
            var token = GenerateJwtToken(user, user.Role.RoleName, sessionValue);

            return new ApiResponse<RiderLoginData>
            {
                Status = true,
                Message = "Login successful",
                Data = new RiderLoginData
                {
                    UserId = user.UserId,
                    Role = user.Role.RoleName,
                    Token = token,
                    SessionValue = sessionValue
                }
            };
        }

        public async Task<ProcedureResponse> RequestRideAsync(RiderRideRequest request)
        {
            var rider = await _riderRepository.GetActiveRiderByIdAsync(request.UserId);
            if (rider == null)
            {
                return new ProcedureResponse
                {
                    Status = false,
                    Message = "Rider is not active or not present in system"
                };
            }

            var pickupLocation = await _riderRepository.GetActiveLocationByNameAsync(request.PickUPLocationName);
            if (pickupLocation == null)
            {
                return new ProcedureResponse
                {
                    Status = false,
                    Message = "Pickup location is not present in system"
                };
            }

            var dropLocation = await _riderRepository.GetActiveLocationByNameAsync(request.DropLocationName);
            if (dropLocation == null)
            {
                return new ProcedureResponse
                {
                    Status = false,
                    Message = "Drop location is not present in system"
                };
            }

            return await _riderRepository.ExecuteRideRequestCleanupAsync(
                request.UserId,
                request.PickUPLocationName,
                request.DropLocationName);
        }

        private async Task<string> GenerateUniqueSessionValueAsync()
        {
            string sessionValue;
            do
            {
                sessionValue = Guid.NewGuid().ToString();
            }
            while (await _riderRepository.SessionValueExistsAsync(sessionValue));

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
