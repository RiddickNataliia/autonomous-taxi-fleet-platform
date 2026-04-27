"use client";

import { useState } from "react";
import { useQuery } from "@apollo/client/react";
import { gql } from "@apollo/client/core";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";

const GET_VEHICLES = gql`
  query GetVehiclesForTelemetry {
    vehicles {
      id
      modelName
      licensePlate
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
}

interface TelemetryPoint {
  timestamp: string;
  speed: number;
  batteryPercentage: number;
}

interface VehiclesData {
  vehicles: Vehicle[];
}

interface TelemetryData {
  telemetry: TelemetryPoint[];
}

export default function TelemetryPage() {
  const [selectedVehicleId, setSelectedVehicleId] = useState<string>("");

  const { data: vehiclesData } = useQuery<VehiclesData>(GET_VEHICLES);
  const { data: telemetryData } = useQuery<TelemetryData>(GET_TELEMETRY, {
    variables: { vehicleId: selectedVehicleId, limit: 20 },
    skip: !selectedVehicleId,
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
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between px-6 py-4 bg-white border-b border-gray-100">
        <div>
          <h1 className="text-base font-medium text-gray-900">Telemetry</h1>
          <p className="text-xs text-gray-400 mt-0.5">Live vehicle data · updates every 5s</p>
        </div>
        <select
          value={selectedVehicleId}
          onChange={(e) => setSelectedVehicleId(e.target.value)}
          className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-gray-300"
        >
          <option value="">Select a vehicle</option>
          {vehicles.map((v) => (
            <option key={v.id} value={v.id}>
              {v.modelName} · {v.licensePlate}
            </option>
          ))}
        </select>
      </div>

      <div className="p-6 flex flex-col gap-4 flex-1 min-h-0">
        {!selectedVehicleId && (
          <div className="flex items-center justify-center flex-1 text-sm text-gray-400">
            Select a vehicle to view telemetry
          </div>
        )}

        {selectedVehicleId && chartData.length === 0 && (
          <div className="flex items-center justify-center flex-1 text-sm text-gray-400">
            No telemetry data yet
          </div>
        )}

        {chartData.length > 0 && (
          <>
            <div className="grid grid-cols-3 gap-3">
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="text-xs text-gray-400 mb-1">Current speed</div>
                <div className="text-2xl font-medium text-gray-900">
                  {latest?.speed ?? 0} km/h
                </div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="text-xs text-gray-400 mb-1">Battery</div>
                <div className="text-2xl font-medium text-gray-900">
                  {latest?.battery ?? 0}%
                </div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="text-xs text-gray-400 mb-1">Data points</div>
                <div className="text-2xl font-medium text-gray-900">
                  {telemetry.length}
                </div>
              </div>
            </div>

            <div className="bg-white border border-gray-100 rounded-xl p-4 flex-1 min-h-0">
              <div className="text-sm font-medium text-gray-900 mb-4">
                Speed & Battery over time
              </div>
              <div style={{ width: "100%", height: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                    <XAxis dataKey="label" tick={{ fontSize: 11 }} />
                    <YAxis tick={{ fontSize: 11 }} />
                    <Tooltip />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="speed"
                      stroke="#1D9E75"
                      strokeWidth={2}
                      dot={false}
                      name="Speed (km/h)"
                    />
                    <Line
                      type="monotone"
                      dataKey="battery"
                      stroke="#378ADD"
                      strokeWidth={2}
                      dot={false}
                      name="Battery (%)"
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}