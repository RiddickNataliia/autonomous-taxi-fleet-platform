namespace NovaDrive.Application.Mappings;

public static class VehicleMapping
{
    public static VehicleResponse ToResponse(this Vehicle v) => new(
        VehicleId:          v.Id,
        Vin:                v.VIN.Value,
        LicensePlate:       v.LicensePlate,
        ModelName:          v.ModelName,
        YearOfManufacture:  v.YearOfManufacture,
        Type:               v.Type.ToString(),
        Status:             v.Status.ToString(),
        LocationLatitude:   v.CurrentLocation.Latitude,
        LocationLongitude:  v.CurrentLocation.Longitude,
        BatteryPercentage:  v.Battery.Percentage,
        LastInspectionDate: v.LastInspectionDate);
}