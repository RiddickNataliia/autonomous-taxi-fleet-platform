namespace NovaDrive.Api.Endpoints;

public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPaymentEndpoints(this RouteGroupBuilder group)
    {
        // POST /api/v1/payments — process payment for a completed ride
        group.MapPost("/", async (
            HttpContext              context,
            ProcessPaymentRequest    request,
            IPaymentService          paymentService,
            IUserProvisioningService provisioning,
            CancellationToken        ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");
            var email = context.User.Email()
                ?? throw new UnauthorizedAccessException("Missing email claim.");

            var (passenger, _) = await provisioning.EnsurePassengerExists(auth0UserId, email, ct);
            var transaction = await paymentService.ProcessPayment(request, passenger.PassengerId, ct);
            return Results.Created($"/api/v1/payments/{transaction.RideId}", transaction);
        })
        .RequireAuthorization("create:rides")
        .WithName("ProcessPayment")
        .WithTags("Payments");

        // GET /api/v1/payments/{rideId} — get transaction for a ride
        group.MapGet("/{rideId:guid}", async (
            Guid              rideId,
            IPaymentService   paymentService,
            CancellationToken ct) =>
        {
            var transaction = await paymentService.GetTransactionByRide(rideId, ct);
            return Results.Ok(transaction);
        })
        .RequireAuthorization("admin:support")
        .WithName("GetTransactionByRide")
        .WithTags("Payments");

        return group;
    }
}