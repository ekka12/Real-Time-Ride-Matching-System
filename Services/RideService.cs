using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssignmentApi.Data;
using Microsoft.EntityFrameworkCore;

namespace AssignmentApi.Services
{
    public class RideService : IRideService
    {
        private const double MaxPickupRadiusKm = 15.0;
        private const double MaxDropRadiusKm = 100.0;
        private readonly AssignmentDbContext _context;

        public RideService(AssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateDriverOnlineStatusAsync(int userId, string activeOnline)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Role == null || user.Role.RoleName != "Driver")
            {
                return false;
            }

            user.UserActiveOnline = activeOnline;
            user.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserLocationAsync(int userId, int locationId)
        {
            var user = await _context.Users.FindAsync(userId);
            var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == locationId && l.Active == "Y");

            if (user == null || !locationExists)
            {
                return false;
            }

            user.LocationId = locationId;
            user.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message, IEnumerable<object> Drivers)> GetNearbyDriversAsync(int userId, int? pickupLocationId)
        {
            var rider = await _context.Users
                .AsNoTracking()
                .Include(u => u.Location)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (rider == null || rider.Role == null || rider.Role.RoleName != "Rider")
            {
                return (false, "Only Rider users can search nearby drivers.", Enumerable.Empty<object>());
            }

            var riderLocation = pickupLocationId.HasValue
                ? await _context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.LocationId == pickupLocationId.Value && l.Active == "Y")
                : rider.Location;

            if (riderLocation == null)
            {
                return (false, "Rider pickup location is missing or inactive.", Enumerable.Empty<object>());
            }

            var drivers = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Location)
                .Where(u => u.Role != null
                    && u.Role.RoleName == "Driver"
                    && u.UserActiveOnline == "Y"
                    && u.Location != null
                    && u.Location.Active == "Y")
                .ToListAsync();

            var nearbyDrivers = drivers
                .Select(driver => new
                {
                    driver.UserId,
                    driver.UserEmail,
                    driver.UserPhoneNo,
                    LocationId = driver.LocationId,
                    LocationName = driver.Location!.LocationName,
                    DistanceKm = Math.Round(CalculateDistance(
                        (double)riderLocation.Latitude,
                        (double)riderLocation.Longitude,
                        (double)driver.Location.Latitude,
                        (double)driver.Location.Longitude), 2)
                })
                .Where(driver => driver.DistanceKm <= MaxPickupRadiusKm)
                .OrderBy(driver => driver.DistanceKm)
                .Cast<object>()
                .ToList();

            return (true, "Nearby drivers fetched successfully.", nearbyDrivers);
        }

        public async Task<(bool IsValid, string Message, double DistanceKm)> ValidateRideRequestAsync(int pickupLocationId, int dropLocationId)
        {
            var pickup = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.LocationId == pickupLocationId && l.Active == "Y");
            var drop = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.LocationId == dropLocationId && l.Active == "Y");

            if (pickup == null || drop == null)
            {
                return (false, "Pickup or drop location is missing or inactive.", 0);
            }

            var distance = CalculateDistance(
                (double)pickup.Latitude,
                (double)pickup.Longitude,
                (double)drop.Latitude,
                (double)drop.Longitude);
            if (distance > MaxDropRadiusKm)
            {
                return (false, $"Drop distance exceeds {MaxDropRadiusKm} KM.", Math.Round(distance, 2));
            }

            return (true, "Ride request is valid.", Math.Round(distance, 2));
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0)
                    + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                    * Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

            var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return earthRadiusKm * c;
        }

        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
