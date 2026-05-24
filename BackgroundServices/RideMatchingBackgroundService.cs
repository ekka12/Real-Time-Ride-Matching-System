using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssignmentApi.Data;
using AssignmentApi.Hubs;
using AssignmentApi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AssignmentApi.BackgroundServices
{
    public class RideMatchingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<RideHub> _hubContext;
        private readonly ILogger<RideMatchingBackgroundService> _logger;
        private readonly TimeSpan _matchingInterval = TimeSpan.FromSeconds(5);

        public RideMatchingBackgroundService(
            IServiceScopeFactory scopeFactory,
            IHubContext<RideHub> hubContext,
            ILogger<RideMatchingBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ride Matching Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MatchPendingRidesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Ride Matching Background Service.");
                }

                await Task.Delay(_matchingInterval, stoppingToken);
            }

            _logger.LogInformation("Ride Matching Background Service is stopping.");
        }

        private async Task MatchPendingRidesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AssignmentDbContext>();

            // 1. Fetch pending ride requests
            var pendingRides = await context.RideRequests
                .Include(r => r.Rider)
                .Where(r => r.Status == "Pending")
                .ToListAsync();

            if (!pendingRides.Any())
            {
                return;
            }

            _logger.LogInformation("Found {Count} pending ride request(s) to match.", pendingRides.Count);

            // 2. Fetch available drivers (must be online, available, and have an active SessionId)
            var availableDrivers = await context.DriverLocations
                .Include(dl => dl.Driver)
                .Where(dl => dl.IsAvailable && dl.Driver != null && !string.IsNullOrEmpty(dl.Driver.SessionValue))
                .ToListAsync();

            if (!availableDrivers.Any())
            {
                _logger.LogWarning("No available online drivers found for matching.");
                return;
            }

            foreach (var ride in pendingRides)
            {
                if (!availableDrivers.Any())
                {
                    break;
                }

                // 3. Find the nearest driver using Haversine formula
                DriverLocation? closestDriver = null;
                double closestDistance = double.MaxValue;

                foreach (var driverLoc in availableDrivers)
                {
                    double distance = CalculateDistance(
                        ride.PickupLatitude, ride.PickupLongitude,
                        driverLoc.Latitude, driverLoc.Longitude);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestDriver = driverLoc;
                    }
                }

                if (closestDriver != null)
                {
                    // 4. Assign the driver and update statuses
                    ride.DriverId = closestDriver.DriverId;
                    ride.Status = "Matched";
                    ride.UpdatedAt = DateTime.UtcNow;

                    closestDriver.IsAvailable = false; // Mark driver as unavailable (assigned)
                    closestDriver.UpdatedAt = DateTime.UtcNow;

                    // Update local context
                    context.RideRequests.Update(ride);
                    context.DriverLocations.Update(closestDriver);

                    // Remove from local list of available drivers for this matching tick
                    availableDrivers.Remove(closestDriver);

                    await context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Matched Ride ID {RideId} (Rider: {Rider}) with Driver ID {DriverId} (Driver: {Driver}) at distance {Distance:F2} km.",
                        ride.Id, ride.Rider?.UserEmail, ride.DriverId, closestDriver.Driver?.UserEmail, closestDistance);

                    // 5. Send real-time updates via SignalR
                    if (ride.Rider != null)
                    {
                        await _hubContext.Clients.Group($"User_{ride.RiderId}").SendAsync("RideMatched", new
                        {
                            rideId = ride.Id,
                            driverId = closestDriver.DriverId,
                            driverName = closestDriver.Driver?.UserEmail,
                            distanceKm = closestDistance
                        });
                    }

                    if (closestDriver.Driver != null)
                    {
                        await _hubContext.Clients.Group($"User_{closestDriver.DriverId}").SendAsync("RideAssigned", new
                        {
                            rideId = ride.Id,
                            riderId = ride.RiderId,
                            riderName = ride.Rider?.UserEmail,
                            pickupLatitude = ride.PickupLatitude,
                            pickupLongitude = ride.PickupLongitude,
                            dropoffLatitude = ride.DropoffLatitude,
                            dropoffLongitude = ride.DropoffLongitude
                        });
                    }
                }
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371.0; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

            var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return R * c;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
