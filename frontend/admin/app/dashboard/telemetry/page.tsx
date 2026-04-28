"use client";

import { useState } from "react";
import { useQuery } from "@apollo/client/react";
import { gql } from "@apollo/client/core";
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid,
  Tooltip, Legend, ResponsiveContainer,
} from "recharts";

const GET_VEHICLES = gql`
  query GetVehiclesForTelemetry {
    vehicles {
      id
      modelName
      licensePlate
      status
    }
  }
`;

const GET_TELEMETRY = gql`
  query GetTelemetry($vehicleId: UUID!, $limit: Int!) {
    telemetry(vehicleId: $vehicleId, limit: $limit) {
      timestamp
      speed
      batteryPercentage
    }
  }
`;

interface Vehicle {
  id: string;
  modelName: string;
  licensePlate: string;
  status: string;
}

interface TelemetryPoint {
  timestamp: string;
  speed: number;
  batteryPercentage: number;
}

interface VehiclesData { vehicles: Vehicle[]; }
interface TelemetryData { telemetry: TelemetryPoint[]; }

const statusColor = (s: string) => {
  switch (s) {
    case "ACTIVE":      return "bg-green-50 text-green-700";
    case "EN_ROUTE":    return "bg-blue-50 text-blue-700";
    case "MAINTENANCE": return "bg-yellow-50 text-yellow-700";
    default:            return "bg-gray-100 text-gray-500";
  }
};

export default function TelemetryPage() {
  const [selectedVehicle, setSelectedVehicle] = useState<Vehicle | null>(null);

  const { data: vehiclesData, loading } = useQuery<VehiclesData>(GET_VEHICLES);
  const { data: telemetryData } = useQuery<TelemetryData>(GET_TELEMETRY, {
    variables: { vehicleId: selectedVehicle?.id, limit: 20 },
    skip: !selectedVehicle,
    pollInterval: 5000,
  });

  const vehicles = vehiclesData?.vehicles ?? [];
  const telemetry = telemetryData?.telemetry ?? [];

  const chartData = [...telemetry].reverse().map((t, i) => ({
    index: i,
    speed: Math.round(t.speed),
    battery: t.batteryPercentage,
    label: new Date(t.timestamp).toLocaleTimeString(),
  }));

  const latest = chartData[chartData.length - 1];

  return (
    <div className="flex h-full">
      {/* Vehicle list */}
      <div className={`flex flex-col ${selectedVehicle ? "w-1/3" : "w-full"} border-r border-gray-100 transition-all`}>
        <div className="px-6 py-4 bg-white border-b border-gray-100">
          <h1 className="text-base font-medium text-gray-900">Telemetry</h1>
          <p className="text-xs text-gray-400 mt-0.5">Live vehicle data · updates every 5s</p>
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

      {/* Telemetry panel */}
      {selectedVehicle && (
        <div className="flex-1 flex flex-col min-w-0">
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-white">
            <div>
              <span className="text-sm font-medium text-gray-900">{selectedVehicle.modelName}</span>
              <span className="text-xs text-gray-400 ml-2">{selectedVehicle.licensePlate}</span>
            </div>
            <button
              onClick={() => setSelectedVehicle(null)}
              className="text-gray-400 hover:text-gray-600 text-lg leading-none"
            >
              ×
            </button>
          </div>

          <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
            {telemetry.length === 0 && (
              <div className="flex items-center justify-center flex-1 text-sm text-gray-400">
                No telemetry data yet
              </div>
            )}

            {telemetry.length > 0 && (
              <>
                <div className="grid grid-cols-3 gap-3">
                  <div className="bg-gray-50 rounded-lg p-4">
                    <div className="text-xs text-gray-400 mb-1">Current speed</div>
                    <div className="text-2xl font-medium text-gray-900">{latest?.speed ?? 0} km/h</div>
                  </div>
                  <div className="bg-gray-50 rounded-lg p-4">
                    <div className="text-xs text-gray-400 mb-1">Battery</div>
                    <div className="text-2xl font-medium text-gray-900">{latest?.battery ?? 0}%</div>
                  </div>
                  <div className="bg-gray-50 rounded-lg p-4">
                    <div className="text-xs text-gray-400 mb-1">Data points</div>
                    <div className="text-2xl font-medium text-gray-900">{telemetry.length}</div>
                  </div>
                </div>

                <div className="bg-white border border-gray-100 rounded-xl p-4 flex-1">
                  <div className="text-sm font-medium text-gray-900 mb-4">Speed & Battery over time</div>
                  <div style={{ width: "100%", height: 300 }}>
                    <ResponsiveContainer width="100%" height="100%">
                      <LineChart data={chartData}>
                        <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                        <XAxis dataKey="label" tick={{ fontSize: 11 }} />
                        <YAxis tick={{ fontSize: 11 }} />
                        <Tooltip />
                        <Legend />
                        <Line type="monotone" dataKey="speed" stroke="#1D9E75" strokeWidth={2} dot={false} name="Speed (km/h)" />
                        <Line type="monotone" dataKey="battery" stroke="#378ADD" strokeWidth={2} dot={false} name="Battery (%)" />
                      </LineChart>
                    </ResponsiveContainer>
                  </div>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}