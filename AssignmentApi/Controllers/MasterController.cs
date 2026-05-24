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
    public class MasterController : ControllerBase
    {
        private readonly IMasterService _masterService;

        public MasterController(IMasterService masterService)
        {
            _masterService = masterService;
        }

        [HttpPost("locations")]
        [AllowAnonymous]
        public async Task<IActionResult> AddLocation([FromBody] AddLocationRequest request)
        {
            var location = await _masterService.AddLocationAsync(request);
            return Ok(location);
        }

        [HttpPost("roles")]
        [AllowAnonymous]
        public async Task<IActionResult> AddRole([FromBody] AddRoleRequest request)
        {
            var role = await _masterService.AddRoleAsync(request);
            return Ok(role);
        }

        [HttpGet("locations")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLocations()
        {
            return Ok(await _masterService.GetAllLocationsAsync());
        }

        [HttpGet("roles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _masterService.GetAllRolesAsync());
        }

        [HttpGet("users")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _masterService.GetAllUsersAsync());
        }

        [HttpGet("users/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _masterService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }

        [HttpGet("users/role/{role}")]
        [Authorize]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            return Ok(await _masterService.GetUsersByRoleAsync(role));
        }

        [HttpDelete("users/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _masterService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound(new { message = "User not found or could not be deleted." });
            }

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
