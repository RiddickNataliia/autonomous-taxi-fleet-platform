namespace NovaDrive.Api.Endpoints;

public static class RideEndpoints
{
    public static RouteGroupBuilder MapRideEndpoints(this RouteGroupBuilder group)
    {
        // GET /api/v1/rides/admin/all — admin sees all rides
        group.MapGet("/admin/all", async (
            IRideService      rideService,
            CancellationToken ct) =>
        {
            var rides = await rideService.GetAllRides(ct);
            return Results.Ok(rides);
        })
        .RequireAuthorization("admin:support")
        .WithName("GetAllRides")
        .WithTags("Rides");

        // GET /api/v1/rides/active — passenger sees their current active ride
        group.MapGet("/active", async (
            HttpContext              context,
            IRideService             rideService,
            IUserProvisioningService provisioning,
            CancellationToken        ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");
            var email = context.User.Email()
                ?? throw new UnauthorizedAccessException("Missing email claim.");

            var (passenger, _) = await provisioning.EnsurePassengerExists(auth0UserId, email, ct);
            var ride = await rideService.GetActiveRide(passenger.PassengerId, ct);

            return ride is null ? Results.NoContent() : Results.Ok(ride);
        })
        .RequireAuthorization("read:rides")
        .WithName("GetActiveRide")
        .WithTags("Rides");
        
        // POST /api/v1/rides — request a ride
        group.MapPost("/", async (
            HttpContext          context,
            RequestRideRequest   request,
            IRideService         rideService,
            IUserProvisioningService provisioning,
            CancellationToken    ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");
            var email = context.User.Email()
                ?? throw new UnauthorizedAccessException("Missing email claim.");

            var (passenger, _) = await provisioning.EnsurePassengerExists(auth0UserId, email, ct);

            var rideRequest = request with { PassengerId = passenger.PassengerId };
            var ride = await rideService.RequestRide(rideRequest, ct);
            return Results.Created($"/api/v1/rides/{ride.RideId}", ride);
        })
        .RequireAuthorization("create:rides")
        .WithName("RequestRide")
        .WithTags("Rides");

        // GET /api/v1/rides — get own ride history
        group.MapGet("/", async (
            HttpContext       context,
            IRideService      rideService,
            IUserProvisioningService provisioning,
            CancellationToken ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");
            var email = context.User.Email()
                ?? throw new UnauthorizedAccessException("Missing email claim.");

            var (passenger, _) = await provisioning.EnsurePassengerExists(auth0UserId, email, ct);
            var rides = await rideService.GetRidesByPassenger(passenger.PassengerId, ct);
            return Results.Ok(rides);
        })
        .RequireAuthorization("read:rides")
        .WithName("GetMyRides")
        .WithTags("Rides");

        // PUT /api/v1/rides/{rideId}/start — vehicle signals it has picked up the passenger
        group.MapPut("/{rideId:guid}/start", async (
            HttpContext       context,
            Guid              rideId,
            IRideService      rideService,
            CancellationToken ct) =>
        {
            var vehicle = context.Items["AuthenticatedVehicle"] as Vehicle;
            if (vehicle is null)
                return Results.Unauthorized();

            var ride = await rideService.StartRide(rideId, ct);
            return Results.Ok(ride);
        })
        .WithName("StartRide")
        .WithTags("Rides");

        // PUT /api/v1/rides/{rideId}/complete — vehicle signals arrival at destination
        group.MapPut("/{rideId:guid}/complete", async (
            HttpContext          context,
            Guid                 rideId,
            CompleteRideRequest  request,
            IRideService         rideService,
            CancellationToken    ct) =>
        {
            var vehicle = context.Items["AuthenticatedVehicle"] as Vehicle;
            if (vehicle is null)
                return Results.Unauthorized();

            var rideRequest = request with { RideId = rideId };
            var ride = await rideService.CompleteRide(rideRequest, ct);
            return Results.Ok(ride);
        })
        .WithName("CompleteRide")
        .WithTags("Rides");

        // PUT /api/v1/rides/{rideId}/cancel — passenger cancels a ride
        group.MapPut("/{rideId:guid}/cancel", async (
            Guid              rideId,
            IRideService      rideService,
            CancellationToken ct) =>
        {
            var ride = await rideService.CancelRide(rideId, ct);
            return Results.Ok(ride);
        })
        .RequireAuthorization("create:rides")
        .WithName("CancelRide")
        .WithTags("Rides");

        return group;
    }
}