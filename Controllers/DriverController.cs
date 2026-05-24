using System.Threading.Tasks;
using AssignmentApi.DTOs;
using AssignmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssignmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] DriverLoginRequest request)
        {
            var response = await _driverService.LoginAsync(request);
            return response.Status ? Ok(response) : BadRequest(response);
        }

        [HttpPost("current-location")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> UpdateCurrentLocation([FromBody] DriverLocationUpdateRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var authenticatedUserId) || authenticatedUserId != request.UserId)
            {
                return BadRequest(new ProcedureResponse
                {
                    Status = false,
                    Message = "Driver can update only own location"
                });
            }

            var response = await _driverService.UpdateCurrentLocationAsync(request);
            return response.Status ? Ok(response) : BadRequest(response);
        }
    }
}
