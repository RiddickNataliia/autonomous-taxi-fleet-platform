namespace NovaDrive.Domain.Enums;

public enum VehicleStatus
{
    Unknown = 0,
    Active = 1,    // Ready to pick up passengers
    Inactive = 2,    // Off-duty
    Maintenance = 3, 
    EnRoute = 4  
}