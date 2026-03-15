namespace NovaDrive.Domain.Services;

public class MaintenancePredictor
{
    /// <summary>
    /// Analyzes internal hardware temperature and sensor diagnostics
    /// </summary>
    public void EvaluateVehicleHealth(Vehicle vehicle, double currentTemp, DiagnosticSeverity severity)
    {
        // If it's a critical hardware failure, force it into Maintenance mode
        if (currentTemp > 90.0 || severity == DiagnosticSeverity.Critical)
        {
            vehicle.MarkForMaintenance();
        }
    }
}