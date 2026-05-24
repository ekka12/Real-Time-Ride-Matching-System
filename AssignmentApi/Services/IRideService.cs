using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssignmentApi.Services
{
    public interface IRideService
    {
        Task<bool> UpdateDriverOnlineStatusAsync(int userId, string activeOnline);
        Task<bool> UpdateUserLocationAsync(int userId, int locationId);
        Task<(bool Success, string Message, IEnumerable<object> Drivers)> GetNearbyDriversAsync(int userId, int? pickupLocationId);
        Task<(bool IsValid, string Message, double DistanceKm)> ValidateRideRequestAsync(int pickupLocationId, int dropLocationId);
    }
}
