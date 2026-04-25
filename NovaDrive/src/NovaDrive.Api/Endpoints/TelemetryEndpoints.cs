namespace NovaDrive.Api.Endpoints;

public static class TelemetryEndpoints
{
    public static RouteGroupBuilder MapTelemetryEndpoints(this RouteGroupBuilder group)
    {
        // POST /api/v1/telemetry — vehicle sends live telemetry
        group.MapPost("/", async (
            HttpContext          context,
            LogTelemetryRequest  request,
            ITelemetryRepository telemetryRepo,
            CancellationToken    ct) =>
        {
            var vehicle = context.Items["AuthenticatedVehicle"] as Vehicle
                ?? throw new UnauthorizedAccessException("Vehicle not authenticated.");

            var telemetry = Telemetry.Create(
                vehicleId:   vehicle.Id,
                location:    new GpsLocation(request.Latitude, request.Longitude),
                speed:       request.SpeedKmh,
                battery:     request.BatteryPercentage,
                temperature: request.HardwareTemperature);

            await telemetryRepo.Add(telemetry, ct);
            return Results.Created($"/api/v1/telemetry/{vehicle.Id}", null);
        })
        .WithName("IngestTelemetry")
        .WithTags("Telemetry");

        // GET /api/v1/telemetry/{vehicleId} — admin views vehicle telemetry
        group.MapGet("/{vehicleId:guid}", async (
            Guid                 vehicleId,
            ITelemetryRepository telemetryRepo,
            CancellationToken    ct,
            int                  limit = 100) =>
        {
            var data = await telemetryRepo.GetByVehicleId(vehicleId, limit, ct);
            return Results.Ok(data.Select(t => t.ToResponse()));
        })
        .RequireAuthorization("admin:fleet")
        .WithName("GetTelemetryByVehicle")
        .WithTags("Telemetry");

        return group;
    }
}