namespace NovaDrive.Domain.ValueObjects;

public record GpsLocation
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public GpsLocation(double latitude, double longitude)
    {
        // Ensure coordinates are physically possible
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");

        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Calculates the distance to another point in kilometers using the Haversine formula.
    /// </summary>
    public double DistanceTo(GpsLocation other)
    {
        const double EarthRadiusKm = 6371.0;

        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private GpsLocation() { }

    private static double ToRadians(double angle) => Math.PI * angle / 180.0;
}