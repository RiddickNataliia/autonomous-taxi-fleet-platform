namespace NovaDrive.Application.Validators;


public class RequestRideValidator : AbstractValidator<RequestRideRequest>
{
    public RequestRideValidator()
    {
        RuleFor(x => x.PassengerId).NotEmpty();
        RuleFor(x => x.Departure).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(300);
        RuleFor(x => x.EstimatedDistanceKm).GreaterThan(0);
        RuleFor(x => x.PassengerLatitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.PassengerLongitude).InclusiveBetween(-180, 180);
    }
}

public class CompleteRideValidator : AbstractValidator<CompleteRideRequest>
{
    public CompleteRideValidator()
    {
        RuleFor(x => x.RideId).NotEmpty();
        RuleFor(x => x.ActualDistanceKm).GreaterThan(0);
        RuleFor(x => x.ActualDurationMinutes).GreaterThan(0);
    }
}

public class CreateMaintenanceLogValidator : AbstractValidator<CreateLogRequest>
{
    public CreateMaintenanceLogValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TechnicianName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0)
            .WithMessage("Cost cannot be negative.");
    }
}

public class RegisterVehicleValidator : AbstractValidator<RegisterVehicleRequest>
{
    public RegisterVehicleValidator()
    {
        RuleFor(x => x.Vin).NotEmpty().Length(17)
            .Matches("^[A-HJ-NPR-Z0-9]{17}$")
            .WithMessage("VIN must be 17 alphanumeric characters (no I, O, or Q).");
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.YearOfManufacture).InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.VehicleType).NotEmpty();
    }
}

public class CreateDiscountCodeValidator : AbstractValidator<CreateDiscountCodeRequest>
{
    public CreateDiscountCodeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$")
            .WithMessage("Code must contain only uppercase letters, digits, hyphens, or underscores.");
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.MinimumRideValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExpirationDate).GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiration date must be in the future.");
    }
}

public class CreateTicketValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketValidator()
    {
        RuleFor(x => x.PassengerId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}

public class UpdateVehicleVitalsValidator : AbstractValidator<UpdateVehicleVitalsRequest>
{
    public UpdateVehicleVitalsValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.BatteryPercentage).InclusiveBetween(0, 100);
    }
}

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.HomeAddress)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.PreferredPaymentMethod)
            .NotEmpty()
            .Must(v => Enum.TryParse<PaymentMethod>(v, ignoreCase: true, out var m) && m != PaymentMethod.Unknown)
            .WithMessage("Payment method must be a valid value (e.g. CreditCard, DebitCard, Cash).");
    }
}
