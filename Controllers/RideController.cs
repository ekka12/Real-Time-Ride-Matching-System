using System.Security.Claims;
using System.Threading.Tasks;
using AssignmentApi.DTOs;
using AssignmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentApi.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    public class RideController : ControllerBase
    {
        private readonly IRideService _rideService;
        private readonly IAuthService _authService;

        public RideController(IRideService rideService, IAuthService authService)
        {
            _rideService = rideService;
            _authService = authService;
        }

        [HttpPost("driver-status")]
        [Authorize]
        public async Task<IActionResult> UpdateDriverStatus([FromBody] UpdateDriverStatusRequest request)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _rideService.UpdateDriverOnlineStatusAsync(userId.Value, request.UserActiveOnline);
            if (!success)
            {
                return BadRequest(new { message = "Only Driver users can update driver online/offline status." });
            }

            return Ok(new { message = "Driver status updated successfully." });
        }

        [HttpPost("user-location")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUserLocation([FromBody] UpdateUserLocationRequest request)
        {
            var userId = GetAuthenticatedUserId() ?? await GetSessionUserIdAsync();
            if (userId == null)
            {
                return Unauthorized(new { message = "Valid JWT token or SessionValue is required." });
            }

            var success = await _rideService.UpdateUserLocationAsync(userId.Value, request.LocationId);
            if (!success)
            {
                return BadRequest(new { message = "User or active location was not found." });
            }

            return Ok(new { message = "User location updated successfully." });
        }

        [HttpPost("nearby-drivers")]
        [Authorize]
        public async Task<IActionResult> GetNearbyDrivers([FromBody] NearbyDriversRequest request)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _rideService.GetNearbyDriversAsync(userId.Value, request.PickupLocationId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message, drivers = result.Drivers });
        }

        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateRideRequest([FromBody] ValidateRideRequestDto request)
        {
            var result = await _rideService.ValidateRideRequestAsync(request.PickupLocationId, request.DropLocationId);
            if (!result.IsValid)
            {
                return BadRequest(new { message = result.Message, distanceKm = result.DistanceKm });
            }

            return Ok(new { message = result.Message, distanceKm = result.DistanceKm });
        }

        private int? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private async Task<int?> GetSessionUserIdAsync()
        {
            var sessionValue = Request.Headers["SessionValue"].ToString();
            return await _authService.ValidateSessionAsync(sessionValue);
        }
    }
}
