namespace NovaDrive.Application.Mappings;

public static class VehicleMapping
{
    public static MaintenanceLogResponse LogToResponse(this MaintenanceLog m) => new(
        LogId:              m.Id,
        VehicleId:          m.VehicleId,
        ServiceDate:        m.ServiceDate,
        Description:        m.Description,
        TechnicianName:     m.TechnicianName,
        Cost:               m.Cost,
        NextServiceMileage: m.NextServiceMileage);
}