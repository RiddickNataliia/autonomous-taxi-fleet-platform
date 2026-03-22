// public async Task PerformInspectionAsync(Guid vehicleId, string techName, decimal cost)
// {
//     var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId);
    
//     // 1. Update the "State" of the vehicle (Postgres)
//     vehicle.RecordInspection();
    
//     // 2. Create the "History" log (Postgres)
//     var log = new MaintenanceLog 
//     { 
//         VehicleId = vehicleId, 
//         Description = "Annual Safety Inspection",
//         TechnicianName = techName,
//         Cost = cost
//     };

//     await _logRepo.AddAsync(log);
//     await _unitOfWork.SaveChangesAsync();
// }