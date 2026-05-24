using System.Threading.Tasks;
using AssignmentApi.DTOs;

namespace AssignmentApi.Services
{
    public interface IRiderService
    {
        Task<ApiResponse<RiderLoginData>> LoginAsync(RiderLoginRequest request);
        Task<ProcedureResponse> RequestRideAsync(RiderRideRequest request);
    }
}
