public class RideMatchingService
{
    private const double MaxMatchDistanceKm = 40.0;

    /// <summary>
    /// Finds the nearest available vehicle within the maximum match distance.
    /// Returns null if no suitable vehicle is found within range.
    /// </summary>
    public Vehicle? FindBestMatch(GpsLocation passengerLocation, IEnumerable<Vehicle> availableVehicles)
    {
        return availableVehicles
            .Where(v => v.Status == VehicleStatus.Active)
            .Where(v => !v.Battery.IsLow)
            .Where(v => passengerLocation.DistanceTo(v.CurrentLocation) <= MaxMatchDistanceKm)
            .OrderBy(v => passengerLocation.DistanceTo(v.CurrentLocation))
            .FirstOrDefault();
    }
}