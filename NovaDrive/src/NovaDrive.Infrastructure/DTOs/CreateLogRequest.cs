namespace NovaDrive.Application.DTOs;

public record CreateLogRequest(
    Guid VehicleId,
    string Description,
    string TechnicianName,
    decimal Cost,
    int? NextServiceMileage
);