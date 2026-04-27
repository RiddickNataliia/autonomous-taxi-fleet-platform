namespace NovaDrive.Application.DTOs;



// Passenger/Profile

public record PassengerResponse(
    Guid PassengerId,
    Guid UserId,
    string FullName,
    string HomeAddress,
    string PreferredPaymentMethod,
    int LoyaltyPoints);

public record UpdateProfileRequest(
    string FullName,
    string HomeAddress,
    string PreferredPaymentMethod);


// Rides

public record RequestRideRequest(
    Guid PassengerId,
    string Departure,
    string Destination,
    double EstimatedDistanceKm,
    double PassengerLatitude,
    double PassengerLongitude,
    string? DiscountCode,
    VehicleType? PreferredVehicleType);

public record RideResponse(
    Guid RideId,
    Guid PassengerId,
    Guid VehicleId,
    string Departure,
    string Destination,
    string Status,
    DateTimeOffset RequestTime,
    DateTimeOffset? CompletedTime,
    double DistanceKm,
    int DurationMinutes,
    decimal NetAmount,
    decimal VatAmount,
    decimal FinalPrice,
    decimal LoyaltyDiscountApplied,
    decimal CodeDiscountApplied,
    int LoyaltyPointsUsed,
    bool IsPaid);

public record CompleteRideRequest(
    Guid RideId,
    double ActualDistanceKm,
    int ActualDurationMinutes,
    string? DiscountCode);


// Payments

public record ProcessPaymentRequest(
    Guid RideId,
    string PaymentMethod);

public record TransactionResponse(
    Guid TransactionId,
    Guid RideId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string Status,
    string? BankReference,
    DateTimeOffset PaymentDate);


// Vehicles

public record RegisterVehicleRequest(
    string Vin,
    string LicensePlate,
    string ModelName,
    int YearOfManufacture,
    string VehicleType);

public record VehicleResponse(
    Guid VehicleId,
    string Vin,
    string LicensePlate,
    string ModelName,
    int YearOfManufacture,
    string Type,
    string Status,
    double LocationLatitude,
    double LocationLongitude,
    int BatteryPercentage,
    DateTimeOffset? LastInspectionDate);

public record UpdateVehicleVitalsRequest(
    Guid VehicleId,
    double Latitude,
    double Longitude,
    int BatteryPercentage);

public record ProvisionApiKeyResponse(
    Guid VehicleId,
    string PlainTextKey,
    string Label,
    DateTimeOffset CreatedAt);

// Maintenance

public record CreateLogRequest(
    Guid VehicleId,
    string Description,
    string TechnicianName,
    decimal Cost,
    int? NextServiceMileage);

public record MaintenanceLogResponse(
    Guid LogId,
    Guid VehicleId,
    DateTimeOffset ServiceDate,
    string Description,
    string TechnicianName,
    decimal Cost,
    int? NextServiceMileage);

// Support tickets

public record CreateTicketRequest(
    Guid PassengerId,
    string Subject,
    string Description);

public record TicketResponse(
    Guid TicketId,
    Guid PassengerId,
    string Subject,
    string Description,
    string Priority,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt);

public record UpdatePriorityRequest(string Priority);

// Discount codes

public record CreateDiscountCodeRequest(
    string Code,
    string Type,
    decimal Value,
    decimal MinimumRideValue,
    DateTimeOffset ExpirationDate);

public record DiscountCodeResponse(
    Guid Id,
    string Code,
    string Type,
    decimal Value,
    decimal MinimumRideValue,
    DateTimeOffset ExpirationDate,
    bool IsActive);

// Sensor diagnostics
public record LogDiagnosticRequest(
    SensorType SensorType,
    string ErrorCode,
    DiagnosticSeverity Severity,
    string? RawSensorData
);

public record SensorDiagnosticResponse(
    Guid Id,
    Guid VehicleId,
    SensorType SensorType,
    string ErrorCode,
    DiagnosticSeverity Severity,
    string? RawSensorData,
    DateTimeOffset Timestamp
);

// Telemetry
public record LogTelemetryRequest(
    double Latitude,
    double Longitude,
    double SpeedKmh,
    int BatteryPercentage,
    double HardwareTemperature);

public record TelemetryResponse(
    Guid VehicleId,
    double Latitude,
    double Longitude,
    double SpeedKmh,
    int BatteryPercentage,
    double HardwareTemperature,
    DateTimeOffset Timestamp);
