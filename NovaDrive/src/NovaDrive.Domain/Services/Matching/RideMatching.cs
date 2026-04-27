public interface IRideMatchingService
{
    Vehicle? FindBestMatch(GpsLocation passengerLocation, IEnumerable<Vehicle> availableVehicles, double estimatedDistanceKm, VehicleType? preferredType = null);
}
public class RideMatchingService : IRideMatchingService
{

    private const double MaxMatchDistanceKm = 20.0;

    /// <summary>
    /// Finds the best matching vehicle for a passenger based on their location and estimated ride distance.
    /// </summary>
    /// <param name="passengerLocation">The GPS location of the passenger.</param>
    /// <param name="availableVehicles">A list of currently available vehicles.</param>
    /// <param name="estimatedDistanceKm">The estimated distance of the ride in kilometers.</param>
    /// <param name="preferredType">The preferred vehicle type, or null if no preference.</param>
    /// <returns>The best matching vehicle, or null if no suitable vehicle is found.</returns>
    public Vehicle? FindBestMatch(GpsLocation passengerLocation, IEnumerable<Vehicle> availableVehicles, double estimatedDistanceKm, VehicleType? preferredType = null)
    {
        return availableVehicles
            .Where(v => v.Status == VehicleStatus.Active)
            .Where(v => preferredType == null || v.Type == preferredType)
            .Select(v => new
            {
                Vehicle = v,
                DistanceToPickup = passengerLocation.DistanceTo(v.CurrentLocation)
            })
            .Where(x => x.DistanceToPickup <= MaxMatchDistanceKm)
            .Where(x => x.Vehicle.Battery.IsSufficientFor(x.DistanceToPickup + estimatedDistanceKm))
            .OrderBy(x => x.DistanceToPickup)
            .Select(x => x.Vehicle)
            .FirstOrDefault();
    }
}