public class RideMatchingService
{
    private const double MaxMatchDistanceKm = 20.0;

    /// <summary>
    /// Finds the nearest available vehicle within the maximum match distance.
    /// Returns null if no suitable vehicle is found within range.
    /// </summary>
    public Vehicle? FindBestMatch(GpsLocation passengerLocation, IEnumerable<Vehicle> availableVehicles, double estimatedDistanceKm)
    {
        return availableVehicles
            .Where(v => v.Status == VehicleStatus.Active)
            .Select(v => new
            {
                Vehicle           = v,
                DistanceToPickup  = passengerLocation.DistanceTo(v.CurrentLocation)
            })
            .Where(x => x.DistanceToPickup <= MaxMatchDistanceKm)
            .Where(x => x.Vehicle.Battery.IsSufficientFor(x.DistanceToPickup + estimatedDistanceKm))
            .OrderBy(x => x.DistanceToPickup)
            .Select(x => x.Vehicle)
            .FirstOrDefault();
    }
}