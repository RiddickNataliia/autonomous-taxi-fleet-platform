namespace NovaDrive.Api.Endpoints;

public static class SensorDiagnosticEndpoints
{
    public static RouteGroupBuilder MapSensorDiagnosticEndpoints(this RouteGroupBuilder group)
    {
        // POST /api/v1/diagnostics — vehicle reports sensor fault
        group.MapPost("/", async (
            HttpContext           context,
            LogDiagnosticRequest  request,
            ISensorDiagnosticService diagnosticService,
            CancellationToken     ct) =>
        {
            var vehicle = context.Items["AuthenticatedVehicle"] as Vehicle
                ?? throw new UnauthorizedAccessException("Vehicle not authenticated.");

            await diagnosticService.LogDiagnostic(vehicle.Id, request);
            return Results.Created($"/api/v1/diagnostics/{vehicle.Id}", null);
        })
        .WithName("LogDiagnostic")
        .WithTags("Diagnostics");

        // GET /api/v1/diagnostics/{vehicleId} — admin views diagnostics
        group.MapGet("/{vehicleId:guid}", async (
            Guid                     vehicleId,
            ISensorDiagnosticService diagnosticService,
            CancellationToken        ct) =>
        {
            var data = await diagnosticService.GetByVehicle(vehicleId);
            return Results.Ok(data);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("GetDiagnosticsByVehicle")
        .WithTags("Diagnostics");

        // GET /api/v1/diagnostics/severity/{severity} — admin filters by severity
        group.MapGet("/severity/{severity}", async (
            string                   severity,
            ISensorDiagnosticService diagnosticService,
            CancellationToken        ct) =>
        {
            if (!Enum.TryParse<DiagnosticSeverity>(severity, ignoreCase: true, out var severityEnum))
                return Results.BadRequest("Invalid severity level.");

            var data = await diagnosticService.GetBySeverity(severityEnum);
            return Results.Ok(data);
        })
        .RequireAuthorization("admin:fleet")
        .WithName("GetDiagnosticsBySeverity")
        .WithTags("Diagnostics");

        return group;
    }
}