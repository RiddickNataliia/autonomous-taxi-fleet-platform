"use client";

import { useQuery } from "@apollo/client/react";
import { gql } from "@apollo/client/core";
import dynamic from "next/dynamic";

const FleetMap = dynamic(() => import("@/components/FleetMap"), { ssr: false });

interface Vehicle {
  id: string;
  modelName: string;
  licensePlate: string;
  status: string;
  type: string;
  battery: { percentage: number };
  currentLocation: { latitude: number; longitude: number };
}

interface Ride {
  id: string;
  departure: string;
  destination: string;
  status: string;
  finalPrice: number | null;
  passengerId: string;
  vehicleId: string;
}

interface FleetData {
  vehicles: Vehicle[];
  rides: Ride[];
}

const GET_FLEET = gql`
  query GetFleet {
    vehicles {
      id
      modelName
      licensePlate
      status
      type
      battery {
        percentage
      }
      currentLocation {
        latitude
        longitude
      }
    }
    rides {
      id
      departure
      destination
      status
      finalPrice
      passengerId
      vehicleId
    }
  }
`;

export default function DashboardPage() {
  const { data, loading } = useQuery<FleetData>(GET_FLEET, {
    pollInterval: 5000,
  });

  const vehicles = data?.vehicles ?? [];
  const rides = data?.rides ?? [];
  const activeVehicles = vehicles.filter((v) => v.status === "ACTIVE");
  const activeRides = rides.filter((r) => r.status === "EN_ROUTE");
  const totalRevenue = rides
    .filter((r) => r.status === "COMPLETED")
    .reduce((sum, r) => sum + (r.finalPrice ?? 0), 0);

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between px-6 py-4 bg-white border-b border-gray-100">
        <div>
          <h1 className="text-base font-medium text-gray-900">Fleet overview</h1>
          <p className="text-xs text-gray-400 mt-0.5">Live · updates every 5s</p>
        </div>
        <div className="text-xs text-gray-400">
          {loading && <span>Refreshing...</span>}
        </div>
      </div>

      <div className="p-6 flex flex-col gap-4 flex-1 min-h-0">
        <div className="grid grid-cols-4 gap-3">
          {[
            { label: "Active vehicles", value: activeVehicles.length, sub: `of ${vehicles.length} total` },
            { label: "En route rides",  value: activeRides.length,   sub: "right now" },
            { label: "Total revenue",   value: `€${totalRevenue.toFixed(2)}`, sub: "completed rides" },
            { label: "Fleet size",      value: vehicles.length,      sub: "registered vehicles" },
          ].map((m) => (
            <div key={m.label} className="bg-gray-50 rounded-lg p-4">
              <div className="text-xs text-gray-400 mb-1">{m.label}</div>
              <div className="text-2xl font-medium text-gray-900">{m.value}</div>
              <div className="text-xs text-gray-400 mt-1">{m.sub}</div>
            </div>
          ))}
        </div>

        <div className="grid grid-cols-5 gap-4 flex-1 min-h-0">
          <div className="col-span-3 bg-white border border-gray-100 rounded-xl overflow-hidden flex flex-col">
            <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
              <span className="text-sm font-medium text-gray-900">Live fleet map</span>
              <span className="text-xs text-gray-400 bg-gray-50 px-2 py-1 rounded-full">
                GraphQL · Ghent
              </span>
            </div>
            <div className="flex-1 min-h-0">
              <FleetMap vehicles={vehicles} />
            </div>
          </div>

          <div className="col-span-2 bg-white border border-gray-100 rounded-xl overflow-hidden flex flex-col">
            <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
              <span className="text-sm font-medium text-gray-900">Rides</span>
              <span className="text-xs text-gray-400 bg-gray-50 px-2 py-1 rounded-full">
                {activeRides.length} en route
              </span>
            </div>
            <div className="flex-1 overflow-y-auto">
              {rides.length === 0 && (
                <div className="flex items-center justify-center h-full text-sm text-gray-400">
                  No rides yet
                </div>
              )}
              {rides.map((ride) => (
                <div key={ride.id} className="px-4 py-3 border-b border-gray-50 last:border-0">
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0">
                      <div className="text-sm font-medium text-gray-900 truncate">
                        {ride.departure}
                      </div>
                      <div className="text-xs text-gray-400 truncate">
                        → {ride.destination}
                      </div>
                    </div>
                    <div className="text-right flex-shrink-0">
                      <div className="text-sm font-medium text-gray-900">
                        {ride.finalPrice ? `€${ride.finalPrice.toFixed(2)}` : "—"}
                      </div>
                      <div className="mt-1">
                        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                          ride.status === "EN_ROUTE"
                            ? "bg-green-50 text-green-700"
                            : ride.status === "REQUESTED"
                            ? "bg-blue-50 text-blue-700"
                            : ride.status === "COMPLETED"
                            ? "bg-gray-100 text-gray-500"
                            : "bg-red-50 text-red-600"
                        }`}>
                          {ride.status.toLowerCase().replace("_", " ")}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}