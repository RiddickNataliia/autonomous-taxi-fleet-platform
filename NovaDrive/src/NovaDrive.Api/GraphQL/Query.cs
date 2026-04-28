namespace NovaDrive.Api.GraphQL;

public class Query
{
    // All rides — filterable by status
    public async Task<IEnumerable<Ride>> GetRides(
        [Service] IRideRepository rideRepo,
        RideStatus? status = null,
        CancellationToken ct = default)
    {
        var rides = await rideRepo.GetAll(ct);
        return status.HasValue
            ? rides.Where(r => r.Status == status.Value)
            : rides;
    }

    // Single ride by ID
    public async Task<Ride?> GetRide(
        Guid id,
        [Service] IRideRepository rideRepo,
        CancellationToken ct = default)
        => await rideRepo.GetById(id, ct);

    // All vehicles — filterable by status or type
    public async Task<IEnumerable<Vehicle>> GetVehicles(
        [Service] IVehicleRepository vehicleRepo,
        VehicleStatus? status = null,
        VehicleType? type = null,
        CancellationToken ct = default)
    {
        var vehicles = await vehicleRepo.GetAll(ct);

        if (status.HasValue)
            vehicles = vehicles.Where(v => v.Status == status.Value);

        if (type.HasValue)
            vehicles = vehicles.Where(v => v.Type == type.Value);

        return vehicles;
    }

    // Single vehicle by ID
    public async Task<Vehicle?> GetVehicle(
        Guid id,
        [Service] IVehicleRepository vehicleRepo,
        CancellationToken ct = default)
        => await vehicleRepo.GetById(id, ct);

    // All passengers
    public async Task<IEnumerable<PassengerResponse>> GetPassengers(
        [Service] IPassengerRepository passengerRepo,
        CancellationToken ct = default)
        {
            var passengers = await passengerRepo.GetAll(ct);
            return passengers.Select(p => p.ToResponse());
        }

    // Single passenger by ID
    public async Task<PassengerResponse?> GetPassenger(
        Guid id,
        [Service] IPassengerRepository passengerRepo,
        CancellationToken ct = default)
        {
            var passenger = await passengerRepo.GetById(id, ct);
            return passenger?.ToResponse();
        }

    // Telemetry for a vehicle
    public async Task<IEnumerable<Telemetry>> GetTelemetry(
        Guid vehicleId,
        [Service] ITelemetryRepository telemetryRepo,
        int limit = 50,
        CancellationToken ct = default)
        => await telemetryRepo.GetByVehicleId(vehicleId, limit, ct);

    // Diagnostics for a vehicle
    public async Task<IEnumerable<SensorDiagnostic>> GetDiagnostics(
        Guid vehicleId,
        [Service] ISensorDiagnosticRepository diagnosticRepo,
        CancellationToken ct = default)
        => await diagnosticRepo.GetByVehicleId(vehicleId, ct);
}