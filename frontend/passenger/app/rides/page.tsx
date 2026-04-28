"use client";

import { useState, useEffect, useRef } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { useRouter } from "next/navigation";
import BottomNav from "@/components/BottomNav";

interface Ride {
  rideId: string;
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
  requestTime: string;
  completedTime: string | null;
  isPaid: boolean;
  vehicleId: string;
}

const statusColor = (s: string) => {
  switch (s) {
    case "EnRoute":   return "text-green-600 bg-green-50";
    case "Requested": return "text-blue-600 bg-blue-50";
    case "Completed": return "text-gray-500 bg-gray-100";
    case "Canceled":  return "text-red-500 bg-red-50";
    default:          return "text-gray-500 bg-gray-100";
  }
};

const statusLabel = (s: string) => {
  switch (s) {
    case "EnRoute":   return "En route";
    case "Requested": return "Finding vehicle...";
    case "Completed": return "Completed";
    case "Canceled":  return "Cancelled";
    default:          return s;
  }
};

export default function RidesPage() {
  const { isAuthenticated, isLoading } = useAuth0();
  const router = useRouter();
  const [rides, setRides] = useState<Ride[]>([]);
  const [loading, setLoading] = useState(true);
  const [canceling, setCanceling] = useState<string | null>(null);
  const [cancelConfirm, setCancelConfirm] = useState<string | null>(null);
  const [selectedRide, setSelectedRide] = useState<Ride | null>(null);
  const [paying, setPaying] = useState(false);
  const [paymentSuccess, setPaymentSuccess] = useState(false);
  const initialLoadDone = useRef(false);

  const token = typeof window !== "undefined"
    ? localStorage.getItem("passenger_token")
    : null;

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.push("/login");
  }, [isAuthenticated, isLoading]);

  const fetchRides = async () => {
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/v1/rides`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      if (res.ok) {
        const data = await res.json();
        setRides(data);
        if (selectedRide) {
          const updated = data.find((r: Ride) => r.rideId === selectedRide.rideId);
          if (updated) setSelectedRide(updated);
        }
      }
    } catch {
      // silent fail on polling errors
    } finally {
      if (!initialLoadDone.current) {
        initialLoadDone.current = true;
        setLoading(false);
      }
    }
  };

  useEffect(() => {
    fetchRides();
    const interval = setInterval(fetchRides, 5000);
    return () => clearInterval(interval);
  }, []);

  const cancelRide = async (id: string) => {
    setCanceling(id);
    setCancelConfirm(null);
    try {
      await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/rides/${id}/cancel`, {
        method: "PUT",
        headers: { Authorization: `Bearer ${token}` },
      });
      await fetchRides();
    } finally {
      setCanceling(null);
    }
  };

  const payRide = async (ride: Ride) => {
    setPaying(true);
    setPaymentSuccess(false);
    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/payments`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          rideId: ride.rideId,
          paymentMethod: "CreditCard",
        }),
      });
      if (res.ok) {
        setPaymentSuccess(true);
        await fetchRides();
      }
    } finally {
      setPaying(false);
    }
  };

  const activeRide = rides.find((r) => r.status === "Requested" || r.status === "EnRoute");
  const pastRides = rides
    .filter((r) => r.status === "Completed" || r.status === "Canceled")
    .sort((a, b) => new Date(b.requestTime).getTime() - new Date(a.requestTime).getTime());

  return (
    <div className="min-h-screen bg-white pb-20">
      <div className="max-w-md mx-auto px-4">
        <div className="pt-12 pb-6">
          <h1 className="text-xl font-semibold text-gray-900">My rides</h1>
        </div>

        {loading && <div className="text-sm text-gray-400">Loading...</div>}

        {activeRide && (
          <div className="mb-6">
            <div className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-2">
              Active
            </div>
            <div className="bg-gray-900 text-white rounded-2xl p-4">
              <div className="mb-3">
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                  activeRide.status === "EnRoute"
                    ? "bg-green-500 text-white"
                    : "bg-blue-500 text-white"
                }`}>
                  {statusLabel(activeRide.status)}
                </span>
              </div>
              <div className="text-sm font-medium">{activeRide.departure}</div>
              <div className="text-sm text-gray-400 mt-0.5">→ {activeRide.destination}</div>

              {activeRide.status === "Requested" && (
                <>
                  {(() => {
                    const waitMinutes = (Date.now() - new Date(activeRide.requestTime).getTime()) / 60000;
                    return waitMinutes > 1 ? (
                    <div className="mt-3 text-xs text-yellow-400 bg-yellow-400/10 rounded-xl px-3 py-2">
                      Taking longer than usual. No vehicles may be available nearby — you can cancel and try again.
                    </div>
                  ) : null;
                })()}
                  {cancelConfirm === activeRide.rideId ? (
                    <div className="mt-4 space-y-2">
                      <p className="text-xs text-gray-400 text-center">
                        Are you sure you want to cancel?
                      </p>
                      <div className="flex gap-2">
                        <button
                          onClick={() => cancelRide(activeRide.rideId)}
                          disabled={canceling === activeRide.rideId}
                          className="flex-1 bg-red-500 text-white text-xs py-2 rounded-xl disabled:opacity-50"
                        >
                          {canceling === activeRide.rideId ? "Cancelling..." : "Yes, cancel"}
                        </button>
                        <button
                          onClick={() => setCancelConfirm(null)}
                          className="flex-1 border border-gray-600 text-gray-300 text-xs py-2 rounded-xl"
                        >
                          Keep ride
                        </button>
                      </div>
                    </div>
                  ) : (
                    <button
                      onClick={() => setCancelConfirm(activeRide.rideId)}
                      className="mt-4 w-full border border-gray-600 text-gray-300 text-xs py-2 rounded-xl hover:bg-gray-800 transition-colors"
                    >
                      Cancel ride
                    </button>
                  )}
                </>
              )}
            </div>
          </div>
        )}

        {pastRides.length > 0 && (
          <div>
            <div className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-2">
              History
            </div>
            <div className="space-y-2">
              {pastRides.map((r) => (
                <div
                  key={r.rideId}
                  onClick={() => {
                    setSelectedRide(r);
                    setPaymentSuccess(false);
                  }}
                  className="bg-gray-50 rounded-2xl p-4 cursor-pointer hover:bg-gray-100 transition-colors"
                >
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0">
                      <div className="text-sm font-medium text-gray-900 truncate">
                        {r.departure}
                      </div>
                      <div className="text-xs text-gray-400 truncate">
                        → {r.destination}
                      </div>
                      <div className="text-xs text-gray-400 mt-1">
                        {new Date(r.requestTime).toLocaleDateString()}
                      </div>
                    </div>
                    <div className="text-right flex-shrink-0">
                      <div className="text-sm font-medium text-gray-900">
                        {r.finalPrice ? `€${r.finalPrice.toFixed(2)}` : "—"}
                      </div>
                      <div className="flex flex-col items-end gap-1 mt-1">
                        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(r.status)}`}>
                          {statusLabel(r.status)}
                        </span>
                        {r.status === "Completed" && !r.isPaid && (
                          <span className="text-xs px-2 py-0.5 rounded-full font-medium bg-orange-50 text-orange-600">
                            Unpaid
                          </span>
                        )}
                        {r.status === "Completed" && r.isPaid && (
                          <span className="text-xs px-2 py-0.5 rounded-full font-medium bg-green-50 text-green-600">
                            Paid
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {!loading && rides.length === 0 && (
          <div className="text-center py-16">
            <div className="text-4xl mb-3">🚗</div>
            <div className="text-sm text-gray-400">No rides yet</div>
            <button
              onClick={() => router.push("/home")}
              className="mt-4 text-sm text-gray-900 font-medium underline"
            >
              Book your first ride
            </button>
          </div>
        )}
      </div>

      {selectedRide && (
        <div
          className="fixed inset-0 z-50 flex items-end justify-center"
          style={{ backgroundColor: "rgba(0,0,0,0.4)" }}
          onClick={(e) => { if (e.target === e.currentTarget) setSelectedRide(null); }}
        >
          <div className="bg-white rounded-t-3xl w-full max-w-md p-6 max-h-[85vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-base font-semibold text-gray-900">Ride details</h2>
              <button
                onClick={() => setSelectedRide(null)}
                className="text-gray-400 hover:text-gray-600 text-xl leading-none"
              >
                ×
              </button>
            </div>

            <div className="space-y-4">
              <div>
                <div className="text-xs text-gray-400 mb-1">Route</div>
                <div className="text-sm font-medium text-gray-900">{selectedRide.departure}</div>
                <div className="text-sm text-gray-500">→ {selectedRide.destination}</div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div className="bg-gray-50 rounded-xl p-3">
                  <div className="text-xs text-gray-400 mb-1">Status</div>
                  <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(selectedRide.status)}`}>
                    {statusLabel(selectedRide.status)}
                  </span>
                </div>
                <div className="bg-gray-50 rounded-xl p-3">
                  <div className="text-xs text-gray-400 mb-1">Payment</div>
                  <div className={`text-sm font-medium ${selectedRide.isPaid ? "text-green-600" : "text-orange-500"}`}>
                    {selectedRide.isPaid ? "✓ Paid" : "Pending"}
                  </div>
                </div>
                <div className="bg-gray-50 rounded-xl p-3">
                  <div className="text-xs text-gray-400 mb-1">Distance</div>
                  <div className="text-sm font-medium text-gray-900">
                    {selectedRide.distanceKm?.toFixed(1)} km
                  </div>
                </div>
                <div className="bg-gray-50 rounded-xl p-3">
                  <div className="text-xs text-gray-400 mb-1">Duration</div>
                  <div className="text-sm font-medium text-gray-900">
                    {selectedRide.durationMinutes} min
                  </div>
                </div>
              </div>

              <div className="bg-gray-50 rounded-xl p-4 space-y-2">
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
                <div className="border-t border-gray-200 pt-2 flex justify-between">
                  <span className="text-sm font-semibold text-gray-900">Total</span>
                  <span className="text-sm font-semibold text-gray-900">
                    €{selectedRide.finalPrice?.toFixed(2) ?? "—"}
                  </span>
                </div>
              </div>

              <div className="text-xs text-gray-400">
                Requested: {new Date(selectedRide.requestTime).toLocaleString()}
                {selectedRide.completedTime && (
                  <> · Completed: {new Date(selectedRide.completedTime).toLocaleString()}</>
                )}
              </div>

              {selectedRide.status === "Completed" && !selectedRide.isPaid && (
                <div>
                  {paymentSuccess ? (
                    <div className="bg-green-50 border border-green-100 rounded-xl px-4 py-3 text-center">
                      <div className="text-sm font-medium text-green-700">✓ Payment successful</div>
                      <div className="text-xs text-green-600 mt-0.5">
                        Invoice sent to your email
                      </div>
                    </div>
                  ) : (
                    <button
                      onClick={() => payRide(selectedRide)}
                      disabled={paying}
                      className="w-full bg-gray-900 text-white text-sm py-3 rounded-xl hover:bg-gray-800 disabled:opacity-50 transition-colors font-medium"
                    >
                      {paying ? "Processing..." : `Pay €${selectedRide.finalPrice?.toFixed(2)}`}
                    </button>
                  )}
                </div>
              )}

              {selectedRide.status === "Completed" && selectedRide.isPaid && (
                <div className="bg-green-50 border border-green-100 rounded-xl px-4 py-3 text-center">
                  <div className="text-sm font-medium text-green-700">✓ Paid</div>
                  <div className="text-xs text-green-600 mt-0.5">Invoice sent to your email</div>
                </div>
              )}

              <button
                onClick={() => {
                  setSelectedRide(null);
                  router.push(`/support?rideId=${selectedRide.rideId}&route=${encodeURIComponent(selectedRide.departure + ' → ' + selectedRide.destination)}`);
                }}
                className="w-full border border-gray-200 text-gray-600 text-sm py-2.5 rounded-xl hover:bg-gray-50 transition-colors"
              >
                Report an issue with this ride
              </button>
            </div>
          </div>
        </div>
      )}

      <BottomNav />
    </div>
  );
}