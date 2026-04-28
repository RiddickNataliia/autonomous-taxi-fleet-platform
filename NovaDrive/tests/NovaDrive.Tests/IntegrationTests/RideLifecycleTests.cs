namespace NovaDrive.Tests.IntegrationTests;

public class RideLifecycleTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = default!;
    private ApplicationDbContext _ctx = default!;

    private UserRepository _userRepo = default!;
    private PassengerRepository _passengerRepo = default!;
    private VehicleRepository _vehicleRepo = default!;
    private RideRepository _rideRepo = default!;
    private TransactionRepository _transactionRepo = default!;
    private DiscountCodeRepository _discountRepo = default!;
    private MaintenanceLogRepository _maintenanceRepo = default!;

    private UnitOfWork _unitOfWork = default!;
    private PricingEngine _pricingEngine = default!;
    private RideMatchingService _matchingService = default!;
    private RideService _rideService = default!;
    private PaymentService _paymentService = default!;

    public async Task InitializeAsync()
    {
        (_ctx, _container) = await TestDbContextFactory.CreateAsync();

        _userRepo        = new UserRepository(_ctx);
        _passengerRepo   = new PassengerRepository(_ctx);
        _vehicleRepo     = new VehicleRepository(_ctx);
        _rideRepo        = new RideRepository(_ctx);
        _transactionRepo = new TransactionRepository(_ctx);
        _discountRepo    = new DiscountCodeRepository(_ctx);
        _maintenanceRepo = new MaintenanceLogRepository(_ctx);
        _unitOfWork      = new UnitOfWork(_ctx);
        _pricingEngine   = new PricingEngine();
        _matchingService = new RideMatchingService();

        _rideService = new RideService(
            _rideRepo,
            _passengerRepo,
            _vehicleRepo,
            _discountRepo,
            _unitOfWork,
            _pricingEngine,
            _matchingService);

        _paymentService = new PaymentService(
            _transactionRepo,
            _rideRepo,
            _passengerRepo,
            new DemoPaymentGateway(),
            _unitOfWork,
            new NullInvoiceService(),
            new NullEmailService(),
            NullLogger<PaymentService>.Instance);
    }

    private async Task<(Passenger passenger, Vehicle vehicle)> SeedAsync()
    {
        var user = User.Create("lifecycle@test.com", "auth0|lifecycle", UserRole.Passenger);
        var passenger = Passenger.Create(user.Id);
        passenger.UpdateProfile("Test Rider", "1 Test Street", PaymentMethod.CreditCard);
        passenger.EarnPoints(500); // enough for a loyalty discount

        await _userRepo.Add(user);
        await _passengerRepo.Add(passenger);

        var vehicle = Vehicle.Create("1-LFC-001", "Test Car", new Vin("JH4KA7650MC030001"), 2023, VehicleType.Standard);
        vehicle.UpdateVitals(new GpsLocation(50.8503, 4.3517), new BatteryLevel(80));
        vehicle.RecordInspection();
        vehicle.Activate();
        await _vehicleRepo.Add(vehicle);

        await _ctx.SaveChangesAsync();
        return (passenger, vehicle);
    }

    [Fact]
    public async Task FullRideLifecycle_StandardRide_ProducesCorrectPricingAndPoints()
    {
        // Arrange
        var (passenger, vehicle) = await SeedAsync();
        var initialPoints = passenger.LoyaltyPoints; // 500

        // ACT 1: Request ride
        var rideResponse = await _rideService.RequestRide(new RequestRideRequest(
            PassengerId:          passenger.Id,
            Departure:            "Brussels Central",
            Destination:          "Brussels Airport",
            EstimatedDistanceKm:  15.0,
            PassengerLatitude:    50.8503,
            PassengerLongitude:   4.3517,
            DiscountCode:         null,
            PreferredVehicleType: null));

        rideResponse.Status.Should().Be(RideStatus.Requested.ToString());
        rideResponse.VehicleId.Should().Be(vehicle.Id);

        // ACT 2: Start ride
        var startedRide = await _rideService.StartRide(rideResponse.RideId);
        startedRide.Status.Should().Be(RideStatus.EnRoute.ToString());

        // ACT 3: Complete ride — 10 km, 20 min, daytime, 500 loyalty points
        // Base: €2.50 + (10 × €1.10) + (20 × €0.30) = €19.50
        // Loyalty: 500 pts = €5.00, cap = €19.50 × 20% = €3.90 → floor = €3.00
        // Net: €19.50 - €3.00 = €16.50
        // VAT: €16.50 × 21% = €3.47 → Gross: €19.97
        var completedRide = await _rideService.CompleteRide(new CompleteRideRequest(
            RideId:               rideResponse.RideId,
            ActualDistanceKm:     10.0,
            ActualDurationMinutes: 20,
            DiscountCode:         null));

        completedRide.Status.Should().Be(RideStatus.Completed.ToString());
        completedRide.NetAmount.Should().Be(16.50m);
        completedRide.VatAmount.Should().Be(3.47m);
        completedRide.FinalPrice.Should().Be(19.97m);
        completedRide.LoyaltyDiscountApplied.Should().Be(3.00m);
        completedRide.LoyaltyPointsUsed.Should().Be(300);

        // Points after: 500 - 300 (spent) + earned (19.97 × 10 = 199 points)
        var updatedPassenger = await _passengerRepo.GetById(passenger.Id);
        updatedPassenger!.LoyaltyPoints.Should().Be(500 - 300 + 199);

        // ACT 4: Process payment
        var transaction = await _paymentService.ProcessPayment(new ProcessPaymentRequest(
            RideId:        rideResponse.RideId,
            PaymentMethod: "CreditCard"),
            passenger.Id);

        transaction.Status.Should().Be(TransactionStatus.Successful.ToString());
        transaction.Amount.Should().Be(19.97m);
        transaction.RideId.Should().Be(rideResponse.RideId);

        var paidRide = await _rideRepo.GetById(rideResponse.RideId);
        paidRide!.IsPaid.Should().BeTrue();
    }

    [Fact]
    public async Task FullRideLifecycle_WithDiscountCode_AppliesAfterLoyalty()
    {
        // Arrange
        var (passenger, vehicle) = await SeedAsync();

        var code = DiscountCode.Create("TESTCODE", DiscountType.Flat, 5.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30));
        await _discountRepo.Add(code);
        await _ctx.SaveChangesAsync();

        // Request → Start → Complete with discount code
        var rideResponse = await _rideService.RequestRide(new RequestRideRequest(
            PassengerId:          passenger.Id,
            Departure:            "A",
            Destination:          "B",
            EstimatedDistanceKm:  10.0,
            PassengerLatitude:    50.8503,
            PassengerLongitude:   4.3517,
            DiscountCode:         null,
            PreferredVehicleType: null));

        await _rideService.StartRide(rideResponse.RideId);

        // Base: €19.50, loyalty €3.00 off → €16.50, flat €5.00 off → €11.50
        // VAT: €11.50 × 21% = €2.42 → Gross: €13.92
        var completedRide = await _rideService.CompleteRide(new CompleteRideRequest(
            RideId:                rideResponse.RideId,
            ActualDistanceKm:      10.0,
            ActualDurationMinutes: 20,
            DiscountCode:          "TESTCODE"));

        completedRide.NetAmount.Should().Be(11.50m);
        completedRide.CodeDiscountApplied.Should().Be(5.00m);
        completedRide.LoyaltyDiscountApplied.Should().Be(3.00m);
    }

    [Fact]
    public async Task RequestRide_WhenPassengerAlreadyHasActiveRide_Throws()
    {
        var (passenger, vehicle) = await SeedAsync();

        await _rideService.RequestRide(new RequestRideRequest(
            PassengerId:          passenger.Id,
            Departure:            "A",
            Destination:          "B",
            EstimatedDistanceKm:  5.0,
            PassengerLatitude:    50.8503,
            PassengerLongitude:   4.3517,
            DiscountCode:         null,
            PreferredVehicleType: null));

        // Second request should be rejected
        var act = () => _rideService.RequestRide(new RequestRideRequest(
            PassengerId:          passenger.Id,
            Departure:            "C",
            Destination:          "D",
            EstimatedDistanceKm:  5.0,
            PassengerLatitude:    50.8503,
            PassengerLongitude:   4.3517,
            DiscountCode:         null,
            PreferredVehicleType: null));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*active ride*");
    }

    [Fact]
    public async Task Payment_WhenAmountIs666_MarksTransactionAsFailed()
    {
        // The DemoPaymentGateway throws on amount 666 — verifies failure path
        var (passenger, _) = await SeedAsync();

        var rideResponse = await _rideService.RequestRide(new RequestRideRequest(
            PassengerId:          passenger.Id,
            Departure:            "A",
            Destination:          "B",
            EstimatedDistanceKm:  5.0,
            PassengerLatitude:    50.8503,
            PassengerLongitude:   4.3517,
            DiscountCode:         null,
            PreferredVehicleType: null));

        await _rideService.StartRide(rideResponse.RideId);

        // Manually force final price to 666 to trigger gateway failure
        var ride = await _rideRepo.GetById(rideResponse.RideId);
        var result = new PricingResult(666m, 0m, 666m, 0m, 0m, 0);
        ride!.CompleteRide(result, 5.0, 10);
        await _ctx.SaveChangesAsync();

        var transaction = await _paymentService.ProcessPayment(new ProcessPaymentRequest(
            RideId:        rideResponse.RideId,
            PaymentMethod: "CreditCard"),
            passenger.Id);

        transaction.Status.Should().Be(TransactionStatus.Failed.ToString());
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _container.DisposeAsync();
    }
}

// Null implementations to avoid email/PDF side effects in tests
file sealed class NullInvoiceService : NovaDrive.Domain.Interfaces.IInvoiceService
{
    public byte[] GenerateInvoice(string passengerName, string passengerEmail,
        string departure, string destination, double distanceKm, int durationMinutes,
        decimal netAmount, decimal vatAmount, decimal totalAmount,
        decimal loyaltyDiscount, decimal codeDiscount, string paymentMethod,
        string paymentStatus, string? bankReference, DateTimeOffset paymentDate)
        => Array.Empty<byte>();
}

file sealed class NullEmailService : NovaDrive.Domain.Interfaces.IEmailService
{
    public Task SendInvoice(string toEmail, string passengerName, byte[] pdfBytes, CancellationToken ct = default)
        => Task.CompletedTask;
}