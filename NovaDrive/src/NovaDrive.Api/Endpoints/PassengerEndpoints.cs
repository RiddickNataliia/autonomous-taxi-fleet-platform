namespace NovaDrive.Api.Endpoints;

public static class PassengerEndpoints
{
    public static RouteGroupBuilder MapPassengerEndpoints(this RouteGroupBuilder group)
    {

        // Creates local User + Passenger records if this is the first time
        group.MapPost("/me", async (
            HttpContext context,
            IUserProvisioningService provisioning,
            CancellationToken    ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");

            var email = context.User.Email()
                ?? throw new UnauthorizedAccessException("Missing email claim.");

            var (response, isNew) = await provisioning.EnsurePassengerExists(auth0UserId, email, ct);
            return isNew 
                ? Results.Created($"/api/v1/passengers/me", response) 
                : Results.Ok(response);
        })
        .RequireAuthorization("read:profile")
        .WithName("EnsurePassengerProfile")
        .WithTags("Passengers");


        // GET /api/v1/passengers/me
        group.MapGet("/me", async (
            HttpContext       context,
            IPassengerService passengerService,
            CancellationToken ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");

            var response = await passengerService.GetProfile(auth0UserId, ct);
            return Results.Ok(response);
        })
        .RequireAuthorization("read:profile")
        .WithName("GetPassengerProfile")
        .WithTags("Passengers");



        // PUT /api/v1/passengers/me
        group.MapPut("/me", async (
            HttpContext          context,
            UpdateProfileRequest request,
            IPassengerService    passengerService,
            CancellationToken    ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");

            var response = await passengerService.UpdateProfile(auth0UserId, request, ct);
            return Results.Ok(response);
        })
        .RequireAuthorization("update:profile")
        .WithName("UpdatePassengerProfile")
        .WithTags("Passengers");

        // GET /api/v1/passengers — admin sees all passengers
        group.MapGet("/", async (
            IPassengerService passengerService,
            CancellationToken ct) =>
        {
            var passengers = await passengerService.GetAllPassengers(ct);
            return Results.Ok(passengers);
        })
        .RequireAuthorization("admin:users")
        .WithName("GetAllPassengers")
        .WithTags("Passengers");

        return group;
    }
}