using System.Security.Claims;
using System.Threading.Tasks;
using AssignmentApi.DTOs;
using AssignmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiderController : ControllerBase
    {
        private readonly IRiderService _riderService;

        public RiderController(IRiderService riderService)
        {
            _riderService = riderService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] RiderLoginRequest request)
        {
            var response = await _riderService.LoginAsync(request);
            return response.Status ? Ok(response) : BadRequest(response);
        }

        [HttpPost("request-ride")]
        [Authorize(Roles = "Rider")]
        public async Task<IActionResult> RequestRide([FromBody] RiderRideRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var authenticatedUserId) || authenticatedUserId != request.UserId)
            {
                return BadRequest(new ProcedureResponse
                {
                    Status = false,
                    Message = "Rider can request ride only for own user account"
                });
            }

            var response = await _riderService.RequestRideAsync(request);
            return response.Status ? Ok(response) : BadRequest(response);
        }
    }
}
