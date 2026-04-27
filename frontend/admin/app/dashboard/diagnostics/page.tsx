"use client";

import { useState } from "react";
import { useQuery } from "@apollo/client/react";
import { gql } from "@apollo/client/core";

const GET_VEHICLES = gql`
  query GetVehiclesForDiagnostics {
    vehicles {
      id
      modelName
      licensePlate
    }
  }
`;

const GET_VEHICLE_DIAGNOSTICS = gql`
  query GetVehicleDiagnostics($vehicleId: UUID!, ) {
    diagnostics(vehicleId: $vehicleId) {
      id
      sensorType
      errorCode
      severity
      timestamp
    }
  }
`;

interface Vehicle {
  id: string;
  modelName: string;
  licensePlate: string;
}

interface Diagnostic {
  id: string;
  sensorType: string;
  errorCode: string;
  severity: string;
  timestamp: string;
}

interface VehiclesData {
  vehicles: Vehicle[];
}

interface DiagnosticsData {
  diagnostics: Diagnostic[];
}

const severityColor = (s: string) => {
  switch (s) {
    case "Critical": return "bg-red-100 text-red-700";
    case "Error":    return "bg-orange-100 text-orange-700";
    case "Warning":  return "bg-yellow-100 text-yellow-700";
    default:         return "bg-gray-100 text-gray-500";
  }
};

export default function DiagnosticsPage() {
  const [selectedVehicleId, setSelectedVehicleId] = useState<string>("");

  const { data: vehiclesData } = useQuery<VehiclesData>(GET_VEHICLES);
  const { data: diagData } = useQuery<DiagnosticsData>(GET_VEHICLE_DIAGNOSTICS, {
    variables: { vehicleId: selectedVehicleId },
    skip: !selectedVehicleId,
    pollInterval: 10000,
  });

  const vehicles = vehiclesData?.vehicles ?? [];
  const diagnostics = diagData?.diagnostics ?? [];

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between px-6 py-4 bg-white border-b border-gray-100">
        <div>
          <h1 className="text-base font-medium text-gray-900">Sensor diagnostics</h1>
          <p className="text-xs text-gray-400 mt-0.5">Lidar · Radar · Camera</p>
        </div>
        <select
          value={selectedVehicleId}
          onChange={(e) => setSelectedVehicleId(e.target.value)}
          className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none"
        >
          <option value="">Select a vehicle</option>
          {vehicles.map((v) => (
            <option key={v.id} value={v.id}>
              {v.modelName} · {v.licensePlate}
            </option>
          ))}
        </select>
      </div>

      <div className="p-6 flex-1 overflow-auto">
        {!selectedVehicleId && (
          <div className="flex items-center justify-center h-full text-sm text-gray-400">
            Select a vehicle to view diagnostics
          </div>
        )}
        {selectedVehicleId && diagnostics.length === 0 && (
          <div className="flex items-center justify-center h-full text-sm text-gray-400">
            No diagnostics recorded
          </div>
        )}
        {diagnostics.length > 0 && (
          <div className="bg-white border border-gray-100 rounded-xl overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100">
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Sensor</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Error code</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Severity</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Time</th>
                </tr>
              </thead>
              <tbody>
                {diagnostics.map((d) => (
                  <tr key={d.id} className="border-b border-gray-50 last:border-0">
                    <td className="px-4 py-3 font-medium text-gray-900">{d.sensorType}</td>
                    <td className="px-4 py-3 text-gray-500 font-mono text-xs">{d.errorCode}</td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${severityColor(d.severity)}`}>
                        {d.severity}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-400 text-xs">
                      {new Date(d.timestamp).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}