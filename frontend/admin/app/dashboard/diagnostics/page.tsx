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
      status
    }
  }
`;

const GET_VEHICLE_DIAGNOSTICS = gql`
  query GetVehicleDiagnostics($vehicleId: UUID!) {
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
  status: string;
}

interface Diagnostic {
  id: string;
  sensorType: string;
  errorCode: string;
  severity: string;
  timestamp: string;
}

interface VehiclesData { vehicles: Vehicle[]; }
interface DiagnosticsData { diagnostics: Diagnostic[]; }

const severityColor = (s: string) => {
  switch (s) {
    case "Critical": return "bg-red-100 text-red-700";
    case "Error":    return "bg-orange-100 text-orange-700";
    case "Warning":  return "bg-yellow-100 text-yellow-700";
    default:         return "bg-gray-100 text-gray-500";
  }
};

const statusColor = (s: string) => {
  switch (s) {
    case "ACTIVE":      return "bg-green-50 text-green-700";
    case "EN_ROUTE":    return "bg-blue-50 text-blue-700";
    case "MAINTENANCE": return "bg-yellow-50 text-yellow-700";
    default:            return "bg-gray-100 text-gray-500";
  }
};

export default function DiagnosticsPage() {
  const [selectedVehicle, setSelectedVehicle] = useState<Vehicle | null>(null);

  const { data: vehiclesData, loading } = useQuery<VehiclesData>(GET_VEHICLES);
  const { data: diagData } = useQuery<DiagnosticsData>(GET_VEHICLE_DIAGNOSTICS, {
    variables: { vehicleId: selectedVehicle?.id },
    skip: !selectedVehicle,
    pollInterval: 10000,
  });

  const vehicles = vehiclesData?.vehicles ?? [];
  const diagnostics = diagData?.diagnostics ?? [];

  const criticalCount = diagnostics.filter(d => d.severity === "Critical").length;

  return (
    <div className="flex h-full">
      {/* Vehicle list */}
      <div className={`flex flex-col ${selectedVehicle ? "w-1/3" : "w-full"} border-r border-gray-100 transition-all`}>
        <div className="px-6 py-4 bg-white border-b border-gray-100">
          <h1 className="text-base font-medium text-gray-900">Sensor diagnostics</h1>
          <p className="text-xs text-gray-400 mt-0.5">Lidar · Radar · Camera</p>
        </div>

        <div className="flex-1 overflow-auto p-4">
          {loading && <div className="text-sm text-gray-400 px-2">Loading...</div>}
          {vehicles.length === 0 && !loading && (
            <div className="flex items-center justify-center h-full text-sm text-gray-400">
              No vehicles registered
            </div>
          )}
          <div className="flex flex-col gap-2">
            {vehicles.map((v) => (
              <button
                key={v.id}
                onClick={() => setSelectedVehicle(v)}
                className={`text-left px-4 py-3 rounded-xl border transition-colors ${
                  selectedVehicle?.id === v.id
                    ? "bg-gray-900 border-gray-900"
                    : "bg-white border-gray-100 hover:border-gray-200 hover:bg-gray-50"
                }`}
              >
                <div className={`text-sm font-medium ${selectedVehicle?.id === v.id ? "text-white" : "text-gray-900"}`}>
                  {v.modelName}
                </div>
                <div className={`text-xs mt-0.5 ${selectedVehicle?.id === v.id ? "text-gray-300" : "text-gray-400"}`}>
                  {v.licensePlate}
                </div>
                {!selectedVehicle && (
                  <span className={`inline-block mt-1.5 text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(v.status)}`}>
                    {v.status.toLowerCase().replace("_", " ")}
                  </span>
                )}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Diagnostics panel */}
      {selectedVehicle && (
        <div className="flex-1 flex flex-col min-w-0">
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-white">
            <div className="flex items-center gap-3">
              <div>
                <span className="text-sm font-medium text-gray-900">{selectedVehicle.modelName}</span>
                <span className="text-xs text-gray-400 ml-2">{selectedVehicle.licensePlate}</span>
              </div>
              {criticalCount > 0 && (
                <span className="text-xs px-2 py-0.5 rounded-full font-medium bg-red-100 text-red-700">
                  {criticalCount} critical
                </span>
              )}
            </div>
            <button
              onClick={() => setSelectedVehicle(null)}
              className="text-gray-400 hover:text-gray-600 text-lg leading-none"
            >
              ×
            </button>
          </div>

          <div className="flex-1 overflow-auto p-6">
            {diagnostics.length === 0 && (
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
      )}
    </div>
  );
}