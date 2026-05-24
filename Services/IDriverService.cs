using System.Threading.Tasks;
using AssignmentApi.DTOs;

namespace AssignmentApi.Services
{
    public interface IDriverService
    {
        Task<ApiResponse<DriverLoginData>> LoginAsync(DriverLoginRequest request);
        Task<ProcedureResponse> UpdateCurrentLocationAsync(DriverLocationUpdateRequest request);
    }
}
