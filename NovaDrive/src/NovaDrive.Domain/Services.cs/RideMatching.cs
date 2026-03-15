namespace NovaDrive.Domain.Services;

public class RideMatchingService
{
    /// <summary>
    /// Logic to find the best vehicle.
    /// Requirements: Active, nearest, and sufficient battery
    /// </summary>
    public Vehicle? FindBestMatch(GpsLocation passengerLocation, IEnumerable<Vehicle> availableVehicles)
    {
        return availableVehicles
            .Where(v => v.Status == VehicleStatus.Active)
            // Rule: Only cars with enough juice to actually finish a trip
            .Where(v => !v.Battery.IsLow) 

            .OrderBy(v => passengerLocation.DistanceTo(v.CurrentLocation))
            .FirstOrDefault();
    }

    // private double CalculateDistance(GpsLocation p1, GpsLocation p2)
    // {
    //     // Implementation of Haversine formula or simple Euclidean for now
    //     return Math.Sqrt(Math.Pow(p1.Latitude - p2.Latitude, 2) + Math.Pow(p1.Longitude - p2.Longitude, 2));
    // }
}