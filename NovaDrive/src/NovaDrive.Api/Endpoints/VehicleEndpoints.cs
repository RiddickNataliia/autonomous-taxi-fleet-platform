namespace NovaDrive.Api.Endpoints;

public static class VehicleEndpoints
{
    public static RouteGroupBuilder MapVehicleEndpoints(this RouteGroupBuilder group)
    {

        // Admin only
        
        // GET /api/v1/vehicles — get all vehicles (including inactive ones)
        group.MapGet("/", async (
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            var vehicles = await vehicleService.GetAllVehicles(ct);
            return Results.Ok(vehicles);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("GetAllVehicles")
        .WithTags("Vehicles");

        // POST /api/v1/vehicles — register a new vehicle
        group.MapPost("/", async (
            RegisterVehicleRequest request,
            IVehicleService        vehicleService,
            CancellationToken      ct) =>
        {
            var vehicle = await vehicleService.RegisterVehicle(request, ct);
            return Results.Created($"/api/v1/vehicles/{vehicle.VehicleId}", vehicle);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("RegisterVehicle")
        .WithTags("Vehicles");

        // PUT /api/v1/vehicles/vitals — vehicle updates its location and battery
        group.MapPut("/vitals", async (
            HttpContext                context,
            UpdateVehicleVitalsRequest request,
            IVehicleService            vehicleService,
            CancellationToken          ct) =>
        {
            var vehicle = context.Items["AuthenticatedVehicle"] as Vehicle;
            if (vehicle is null)
                return Results.Unauthorized();

            await vehicleService.UpdateVitals(request, ct);
            return Results.NoContent();
        })
        .WithName("UpdateVehicleVitals")
        .WithTags("Vehicles");

        // GET /api/v1/vehicles/{vehicleId} — get vehicle details
        group.MapGet("/{vehicleId:guid}", async (
            Guid              vehicleId,
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            var vehicle = await vehicleService.GetVehicle(vehicleId, ct);
            return Results.Ok(vehicle);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("GetVehicle")
        .WithTags("Vehicles");

        // PUT /api/v1/vehicles/{vehicleId}/activate
        group.MapPut("/{vehicleId:guid}/activate", async (
            Guid              vehicleId,
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            var vehicle = await vehicleService.ActivateVehicle(vehicleId, ct);
            return Results.Ok(vehicle);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("ActivateVehicle")
        .WithTags("Vehicles");

        // PUT /api/v1/vehicles/{vehicleId}/deactivate
        group.MapPut("/{vehicleId:guid}/deactivate", async (
            Guid              vehicleId,
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            var vehicle = await vehicleService.DeactivateVehicle(vehicleId, ct);
            return Results.Ok(vehicle);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("DeactivateVehicle")
        .WithTags("Vehicles");

        // POST /api/v1/vehicles/{vehicleId}/api-key — provision API key
        group.MapPost("/{vehicleId:guid}/api-key", async (
            Guid              vehicleId,
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            var response = await vehicleService.ProvisionApiKey(vehicleId, ct);
            return Results.Ok(response);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("ProvisionApiKey")
        .WithTags("Vehicles");

        // DELETE /api/v1/vehicles/{vehicleId}/api-key — revoke API key
        group.MapDelete("/{vehicleId:guid}/api-key", async (
            Guid              vehicleId,
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            await vehicleService.RevokeApiKey(vehicleId, ct);
            return Results.NoContent();
        })
        .RequireAuthorization("admin:fleet")
        .WithName("RevokeApiKey")
        .WithTags("Vehicles");

        // POST /api/v1/vehicles/{vehicleId}/inspection — record inspection
        group.MapPost("/{vehicleId:guid}/inspection", async (
            Guid              vehicleId,
            CreateLogRequest  request,
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            var log = await vehicleService.RecordInspection(vehicleId, request, ct);
            return Results.Created($"/api/v1/vehicles/{vehicleId}/maintenance", log);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("RecordInspection")
        .WithTags("Vehicles");

        // POST /api/v1/vehicles/maintenance — add maintenance log
        group.MapPost("/maintenance", async (
            CreateLogRequest  request,
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            var log = await vehicleService.AddMaintenanceLog(request, ct);
            return Results.Created($"/api/v1/vehicles/{request.VehicleId}/maintenance", log);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("AddMaintenanceLog")
        .WithTags("Vehicles");

        // GET /api/v1/vehicles/{vehicleId}/maintenance — get maintenance history
        group.MapGet("/{vehicleId:guid}/maintenance", async (
            Guid              vehicleId,
            IVehicleService   vehicleService,
            CancellationToken ct) =>
        {
            var logs = await vehicleService.GetMaintenanceHistory(vehicleId, ct);
            return Results.Ok(logs);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("GetMaintenanceHistory")
        .WithTags("Vehicles");

        return group;
    }
}