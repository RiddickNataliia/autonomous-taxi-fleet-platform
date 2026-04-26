namespace NovaDrive.Api.Grpc;

public sealed class TelemetryGrpcService(
    ITelemetryRepository telemetryRepo,
    IVehicleRepository   vehicleRepo,
    ILogger<TelemetryGrpcService> logger)
    : TelemetryIngest.TelemetryIngestBase
{
    public override async Task StreamTelemetry(
        IAsyncStreamReader<TelemetryFrame> requestStream,
        IServerStreamWriter<TelemetryAck>  responseStream,
        ServerCallContext                  context)
    {
        await foreach (var frame in requestStream.ReadAllAsync(context.CancellationToken))
        {
            if (!Guid.TryParse(frame.VehicleId, out var vehicleId))
            {
                await responseStream.WriteAsync(new TelemetryAck
                {
                    Accepted = false,
                    Message  = "Invalid vehicle_id format."
                });
                continue;
            }

            // Authenticate: look up vehicle and verify BCrypt key
            var vehicle = await vehicleRepo.GetById(vehicleId, context.CancellationToken);

            if (vehicle is null || string.IsNullOrEmpty(vehicle.ApiKeyHash) ||
                !BCrypt.Net.BCrypt.Verify(frame.ApiKey, vehicle.ApiKeyHash))
            {
                logger.LogWarning("gRPC telemetry rejected — auth failed for vehicle {VehicleId}", vehicleId);
                await responseStream.WriteAsync(new TelemetryAck
                {
                    Accepted = false,
                    Message  = "Authentication failed."
                });
                continue;
            }

            var telemetry = Telemetry.Create(
                vehicleId:   vehicle.Id,
                location:    new GpsLocation(frame.Latitude, frame.Longitude),
                speed:       frame.SpeedKmh,
                battery:     frame.BatteryPercentage,
                temperature: frame.HardwareTemperature);

            await telemetryRepo.Add(telemetry, context.CancellationToken);

            logger.LogInformation(
                "gRPC telemetry stored — vehicle {VehicleId} battery={Battery}% speed={Speed}km/h",
                vehicleId, frame.BatteryPercentage, frame.SpeedKmh);

            await responseStream.WriteAsync(new TelemetryAck
            {
                Accepted = true,
                Message  = "Stored."
            });
        }
    }
}