"use client";

import { useState } from "react";
import { useQuery } from "@apollo/client/react";
import { gql } from "@apollo/client/core";

const GET_RIDES = gql`
  query GetAllRides {
    rides {
      id
      departure
      destination
      status
      finalPrice
      netAmount
      vatAmount
      loyaltyDiscountApplied
      codeDiscountApplied
      loyaltyPointsUsed
      distanceKm
      durationMinutes
      isPaid
      passengerId
      vehicleId
      requestTime
      completedTime
    }
  }
`;

interface Ride {
  id: string;
  departure: string;
  destination: string;
  status: string;
  finalPrice: number | null;
  netAmount: number | null;
  vatAmount: number | null;
  loyaltyDiscountApplied: number;
  codeDiscountApplied: number;
  loyaltyPointsUsed: number;
  distanceKm: number;
  durationMinutes: number;
  isPaid: boolean;
  passengerId: string;
  vehicleId: string;
  requestTime: string;
  completedTime: string | null;
}

interface RidesData {
  rides: Ride[];
}

const statusColor = (s: string) => {
  switch (s) {
    case "EN_ROUTE":  return "bg-green-50 text-green-700";
    case "REQUESTED": return "bg-blue-50 text-blue-700";
    case "COMPLETED": return "bg-gray-100 text-gray-500";
    case "CANCELED":  return "bg-red-50 text-red-600";
    default:          return "bg-gray-100 text-gray-500";
  }
};

export default function RidesPage() {
  const [selectedRide, setSelectedRide] = useState<Ride | null>(null);
  const { data, loading } = useQuery<RidesData>(GET_RIDES, {
    pollInterval: 5000,
  });

  const rides = data?.rides ?? [];

  return (
    <div className="flex h-full">
      <div className={`flex flex-col ${selectedRide ? "w-1/2" : "w-full"} transition-all`}>
        <div className="px-6 py-4 bg-white border-b border-gray-100">
          <h1 className="text-base font-medium text-gray-900">All rides</h1>
          <p className="text-xs text-gray-400 mt-0.5">{rides.length} total · click a row to open</p>
        </div>

        <div className="flex-1 overflow-auto p-6">
          {loading && <div className="text-sm text-gray-400">Loading...</div>}
          {rides.length === 0 && !loading && (
            <div className="flex items-center justify-center h-full text-sm text-gray-400">
              No rides yet
            </div>
          )}
          {rides.length > 0 && (
            <div className="bg-white border border-gray-100 rounded-xl overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100">
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Route</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Status</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Price</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Paid</th>
                  </tr>
                </thead>
                <tbody>
                  {rides.map((r) => (
                    <tr
                      key={r.id}
                      onClick={() => setSelectedRide(r)}
                      className={`border-b border-gray-50 last:border-0 cursor-pointer hover:bg-gray-50 transition-colors ${
                        selectedRide?.id === r.id ? "bg-gray-50" : ""
                      }`}
                    >
                      <td className="px-4 py-3">
                        <div className="font-medium text-gray-900 truncate max-w-xs">
                          {r.departure}
                        </div>
                        <div className="text-xs text-gray-400 truncate max-w-xs">
                          → {r.destination}
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(r.status)}`}>
                          {r.status.toLowerCase().replace("_", " ")}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-gray-900">
                        {r.finalPrice ? `€${r.finalPrice.toFixed(2)}` : "—"}
                      </td>
                      <td className="px-4 py-3">
                        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                          r.isPaid ? "bg-green-50 text-green-700" : "bg-orange-50 text-orange-600"
                        }`}>
                          {r.isPaid ? "Paid" : "Unpaid"}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {selectedRide && (
        <div className="w-1/2 border-l border-gray-100 bg-white flex flex-col">
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
            <span className="text-sm font-medium text-gray-900">Ride details</span>
            <button
              onClick={() => setSelectedRide(null)}
              className="text-gray-400 hover:text-gray-600 text-lg leading-none"
            >
              ×
            </button>
          </div>

          <div className="flex-1 overflow-auto p-6 flex flex-col gap-5">
            <div>
              <div className="text-xs text-gray-400 mb-1">Route</div>
              <div className="text-sm font-medium text-gray-900">{selectedRide.departure}</div>
              <div className="text-sm text-gray-500 mt-0.5">→ {selectedRide.destination}</div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-400 mb-1">Status</div>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(selectedRide.status)}`}>
                  {selectedRide.status.toLowerCase().replace("_", " ")}
                </span>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-400 mb-1">Payment</div>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                  selectedRide.isPaid ? "bg-green-50 text-green-700" : "bg-orange-50 text-orange-600"
                }`}>
                  {selectedRide.isPaid ? "✓ Paid" : "Unpaid"}
                </span>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-400 mb-1">Distance</div>
                <div className="text-sm font-medium text-gray-900">
                  {selectedRide.distanceKm?.toFixed(1)} km
                </div>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-400 mb-1">Duration</div>
                <div className="text-sm font-medium text-gray-900">
                  {selectedRide.durationMinutes} min
                </div>
              </div>
            </div>

            <div className="bg-gray-50 rounded-lg p-4 space-y-2">
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-2">
                Pricing breakdown
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Net amount</span>
                <span className="text-gray-900">€{selectedRide.netAmount?.toFixed(2) ?? "—"}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">VAT (21%)</span>
                <span className="text-gray-900">€{selectedRide.vatAmount?.toFixed(2) ?? "—"}</span>
              </div>
              {selectedRide.loyaltyDiscountApplied > 0 && (
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Loyalty discount</span>
                  <span className="text-green-600">-€{selectedRide.loyaltyDiscountApplied.toFixed(2)}</span>
                </div>
              )}
              {selectedRide.codeDiscountApplied > 0 && (
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Promo code</span>
                  <span className="text-green-600">-€{selectedRide.codeDiscountApplied.toFixed(2)}</span>
                </div>
              )}
              {selectedRide.loyaltyPointsUsed > 0 && (
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Points used</span>
                  <span className="text-gray-900">{selectedRide.loyaltyPointsUsed} pts</span>
                </div>
              )}
              <div className="border-t border-gray-100 pt-2 flex justify-between">
                <span className="text-sm font-semibold text-gray-900">Total</span>
                <span className="text-sm font-semibold text-gray-900">
                  €{selectedRide.finalPrice?.toFixed(2) ?? "—"}
                </span>
              </div>
            </div>

            <div className="grid grid-cols-1 gap-2">
              <div>
                <div className="text-xs text-gray-400 mb-1">Passenger ID</div>
                <div className="text-xs text-gray-500 font-mono">{selectedRide.passengerId}</div>
              </div>
              <div>
                <div className="text-xs text-gray-400 mb-1">Vehicle ID</div>
                <div className="text-xs text-gray-500 font-mono">{selectedRide.vehicleId}</div>
              </div>
              <div>
                <div className="text-xs text-gray-400 mb-1">Requested</div>
                <div className="text-xs text-gray-500">
                  {new Date(selectedRide.requestTime).toLocaleString()}
                </div>
              </div>
              {selectedRide.completedTime && (
                <div>
                  <div className="text-xs text-gray-400 mb-1">Completed</div>
                  <div className="text-xs text-gray-500">
                    {new Date(selectedRide.completedTime).toLocaleString()}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}