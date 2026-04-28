namespace NovaDrive.Application.Services;

public interface IPaymentService
{
    Task<TransactionResponse> ProcessPayment(ProcessPaymentRequest request, Guid passengerId, CancellationToken ct = default);
    Task<TransactionResponse> GetTransactionByRide(Guid rideId, CancellationToken ct = default);
}

public sealed class PaymentService : IPaymentService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IRideRepository        _rideRepo;
    private readonly IPassengerRepository   _passengerRepo;
    private readonly IPaymentGateway        _gateway;
    private readonly IUnitOfWork            _unitOfWork;
    private readonly IInvoiceService        _invoiceService;
    private readonly IEmailService          _emailService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ITransactionRepository transactionRepo,
        IRideRepository        rideRepo,
        IPassengerRepository   passengerRepo,
        IPaymentGateway        gateway,
        IUnitOfWork            unitOfWork,
        IInvoiceService        invoiceService,
        IEmailService          emailService,
        ILogger<PaymentService> logger)
    {
        _transactionRepo = transactionRepo;
        _rideRepo        = rideRepo;
        _passengerRepo   = passengerRepo;
        _gateway         = gateway;
        _unitOfWork      = unitOfWork;
        _invoiceService  = invoiceService;
        _emailService    = emailService;
        _logger           = logger;
    }

    public async Task<TransactionResponse> ProcessPayment(
        ProcessPaymentRequest request, Guid passengerId, CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetById(request.RideId, ct)
            ?? throw new KeyNotFoundException(RideDomainException.NotFound);
        
        if (ride.PassengerId != passengerId)
            throw new UnauthorizedAccessException("You are not authorized to pay for this ride.");

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var method)
            || method == PaymentMethod.Unknown)
            throw new UserDomainException(UserDomainException.InvalidPaymentMethod);

        var transaction = Transaction.Create(
            rideId:   ride.Id,
            amount:   ride.FinalPrice,
            currency: Currency.EUR,
            method:   method);

        await _transactionRepo.Add(transaction, ct);

        try
        {
            var reference = await _gateway.ProcessPayment(transaction.Amount, transaction.Currency);
            transaction.MarkAsSuccessful(reference);
            ride.MarkAsPaid();
        }
        catch
        {
            transaction.MarkAsFailed();
        }

        await _unitOfWork.SaveChanges(ct);

        // Send invoice only on successful payment
        if (transaction.Status == TransactionStatus.Successful)
        {
            var passenger = await _passengerRepo.GetByIdWithUser(ride.PassengerId, ct);
            _logger.LogDebug("Processing invoice for passenger {Name} ({Email})", passenger?.FullName, passenger?.User?.Email);
            if (passenger?.User is not null)
            {
                try
                {
                    var invoiceBytes = _invoiceService.GenerateInvoice(
                        passengerName:   passenger.FullName,
                        passengerEmail:  passenger.User.Email,
                        departure:       ride.Departure,
                        destination:     ride.Destination,
                        distanceKm:      ride.DistanceKm,
                        durationMinutes: ride.DurationMinutes,
                        netAmount:       ride.NetAmount,
                        vatAmount:       ride.VatAmount,
                        totalAmount:     transaction.Amount,
                        loyaltyDiscount: ride.LoyaltyDiscountApplied,
                        codeDiscount:    ride.CodeDiscountApplied,
                        paymentMethod:   transaction.PaymentMethod.ToString(),
                        paymentStatus:   transaction.Status.ToString(),
                        bankReference:   transaction.BankReference,
                        paymentDate:     transaction.PaymentDate);

                    await _emailService.SendInvoice(
                        passenger.User.Email,
                        passenger.FullName,
                        invoiceBytes,
                        ct);

                    _logger.LogInformation("Invoice sent successfully to {Email}", passenger?.User?.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate or send invoice for ride {RideId}", ride.Id);
                }
            }
        }

        return transaction.ToResponse();
    }

    public async Task<TransactionResponse> GetTransactionByRide(
        Guid rideId, CancellationToken ct = default)
    {
        var transaction = await _transactionRepo.GetByRideId(rideId, ct)
            ?? throw new KeyNotFoundException("No transaction found for this ride.");

        return transaction.ToResponse();
    }
}