// Interfaces/ISensorDiagnosticRepository.cs
namespace NovaDrive.Domain.Interfaces;

public interface ISensorDiagnosticRepository
{
    Task Add(SensorDiagnostic diagnostic, CancellationToken cancellationToken = default);
    Task<IEnumerable<SensorDiagnostic>> GetByVehicleId(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SensorDiagnostic>> GetBySeverity(DiagnosticSeverity severity, CancellationToken cancellationToken = default);
}