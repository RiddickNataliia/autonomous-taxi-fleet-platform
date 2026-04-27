"use client";

import { useState, useEffect } from "react";
import { useQuery } from "@apollo/client/react";
import { gql } from "@apollo/client/core";

const GET_PASSENGERS = gql`
  query GetPassengers {
    passengers {
      id
      fullName
      loyaltyPoints
      preferredPaymentMethod
    }
  }
`;

interface PassengerSummary {
  id: string;
  fullName: string;
  loyaltyPoints: number;
  preferredPaymentMethod: string;
}

interface PassengersData {
  passengers: PassengerSummary[];
}

interface PassengerDetail {
  passengerId: string;
  userId: string;
  fullName: string;
  homeAddress: string;
  preferredPaymentMethod: string;
  loyaltyPoints: number;
}

export default function PassengersPage() {
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [detail, setDetail] = useState<PassengerDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);

  const { data, loading } = useQuery<PassengersData>(GET_PASSENGERS, {
    pollInterval: 30000,
  });

  const passengers = data?.passengers ?? [];

  const token = typeof window !== "undefined"
    ? localStorage.getItem("admin_token")
    : null;

  useEffect(() => {
    if (!selectedId) return;
    const fetchDetail = async () => {
      setLoadingDetail(true);
      try {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}/api/v1/passengers/${selectedId}`,
          { headers: { Authorization: `Bearer ${token}` } }
        );
        if (res.ok) {
          const data = await res.json();
          setDetail(data);
        }
      } finally {
        setLoadingDetail(false);
      }
    };
    fetchDetail();
  }, [selectedId]);

  return (
    <div className="flex h-full">
      {/* Main list */}
      <div className={`flex flex-col ${selectedId ? "w-1/2" : "w-full"} transition-all`}>
        <div className="px-6 py-4 bg-white border-b border-gray-100">
          <h1 className="text-base font-medium text-gray-900">Passengers</h1>
          <p className="text-xs text-gray-400 mt-0.5">
            {passengers.length} registered · click a row to open
          </p>
        </div>

        <div className="flex-1 overflow-auto p-6">
          {loading && <div className="text-sm text-gray-400">Loading...</div>}
          {passengers.length === 0 && !loading && (
            <div className="flex items-center justify-center h-full text-sm text-gray-400">
              No passengers yet
            </div>
          )}
          {passengers.length > 0 && (
            <div className="bg-white border border-gray-100 rounded-xl overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100">
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Name</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Loyalty points</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Payment</th>
                  </tr>
                </thead>
                <tbody>
                  {passengers.map((p) => (
                    <tr
                      key={p.id}
                      onClick={() => {
                        setSelectedId(p.id);
                        setDetail(null);
                      }}
                      className={`border-b border-gray-50 last:border-0 cursor-pointer hover:bg-gray-50 transition-colors ${
                        selectedId === p.id ? "bg-gray-50" : ""
                      }`}
                    >
                      <td className="px-4 py-3 font-medium text-gray-900">
                        {p.fullName || <span className="text-gray-400">No name set</span>}
                      </td>
                      <td className="px-4 py-3 text-gray-500">{p.loyaltyPoints} pts</td>
                      <td className="px-4 py-3 text-gray-500">
                        {p.preferredPaymentMethod || "—"}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {/* Detail panel */}
      {selectedId && (
        <div className="w-1/2 border-l border-gray-100 bg-white flex flex-col">
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
            <span className="text-sm font-medium text-gray-900">Passenger details</span>
            <button
              onClick={() => { setSelectedId(null); setDetail(null); }}
              className="text-gray-400 hover:text-gray-600 text-lg leading-none"
            >
              ×
            </button>
          </div>

          <div className="flex-1 overflow-auto p-6">
            {loadingDetail && (
              <div className="text-sm text-gray-400">Loading...</div>
            )}
            {detail && (
              <div className="flex flex-col gap-5">
                <div>
                  <div className="text-xs text-gray-400 mb-1">Full name</div>
                  <div className="text-sm font-medium text-gray-900">
                    {detail.fullName || <span className="text-gray-400">Not set</span>}
                  </div>
                </div>

                <div>
                  <div className="text-xs text-gray-400 mb-1">Home address</div>
                  <div className="text-sm text-gray-700">
                    {detail.homeAddress || <span className="text-gray-400">Not set</span>}
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <div className="text-xs text-gray-400 mb-1">Loyalty points</div>
                    <div className="text-2xl font-medium text-gray-900">
                      {detail.loyaltyPoints}
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-400 mb-1">Payment method</div>
                    <div className="text-sm text-gray-700">
                      {detail.preferredPaymentMethod || "—"}
                    </div>
                  </div>
                </div>

                <div>
                  <div className="text-xs text-gray-400 mb-1">Passenger ID</div>
                  <div className="text-xs text-gray-500 font-mono">{detail.passengerId}</div>
                </div>

                <div>
                  <div className="text-xs text-gray-400 mb-1">User ID</div>
                  <div className="text-xs text-gray-500 font-mono">{detail.userId}</div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}