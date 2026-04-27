"use client";

import { useState, useEffect } from "react";

interface Vehicle {
  vehicleId: string;
  vin: string;
  licensePlate: string;
  modelName: string;
  yearOfManufacture: number;
  type: string;
  status: string;
  locationLatitude: number;
  locationLongitude: number;
  batteryPercentage: number;
  lastInspectionDate: string | null;
}

interface MaintenanceLog {
  logId: string;
  description: string;
  technicianName: string;
  cost: number;
  serviceDate: string;
  nextServiceMileage: number;
}

const statusColor = (s: string) => {
  switch (s) {
    case "Active":      return "bg-green-50 text-green-700";
    case "Inactive":    return "bg-gray-100 text-gray-500";
    case "Maintenance": return "bg-yellow-50 text-yellow-700";
    default:            return "bg-gray-100 text-gray-500";
  }
};

const typeColor = (t: string) => {
  switch (t) {
    case "Luxury": return "bg-blue-50 text-blue-700";
    case "Van":    return "bg-orange-50 text-orange-700";
    default:       return "bg-gray-100 text-gray-500";
  }
};

export default function VehiclesPage() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedVehicle, setSelectedVehicle] = useState<Vehicle | null>(null);
  const [maintenance, setMaintenance] = useState<MaintenanceLog[]>([]);
  const [loadingMaintenance, setLoadingMaintenance] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [newApiKey, setNewApiKey] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);
  const [copiedId, setCopiedId] = useState(false);
  const [activeTab, setActiveTab] = useState<"details" | "maintenance">("details");

  const token = typeof window !== "undefined"
    ? localStorage.getItem("admin_token")
    : null;

  const fetchVehicles = async () => {
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/v1/vehicles`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      setVehicles(data);
      if (selectedVehicle) {
        const updated = data.find((v: Vehicle) => v.vehicleId === selectedVehicle.vehicleId);
        if (updated) setSelectedVehicle(updated);
      }
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  const fetchMaintenance = async (vehicleId: string) => {
    setLoadingMaintenance(true);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/v1/vehicles/${vehicleId}/maintenance`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      if (res.ok) setMaintenance(await res.json());
    } finally {
      setLoadingMaintenance(false);
    }
  };

  useEffect(() => {
    fetchVehicles();
    const interval = setInterval(fetchVehicles, 10000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    if (selectedVehicle && activeTab === "maintenance") {
      fetchMaintenance(selectedVehicle.vehicleId);
    }
  }, [selectedVehicle, activeTab]);

  const activate = async (id: string) => {
    setActionLoading(true);
    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/vehicles/${id}/activate`, {
      method: "PUT",
      headers: { Authorization: `Bearer ${token}` },
    });
    await fetchVehicles();
    setActionLoading(false);
  };

  const deactivate = async (id: string) => {
    setActionLoading(true);
    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/vehicles/${id}/deactivate`, {
      method: "PUT",
      headers: { Authorization: `Bearer ${token}` },
    });
    await fetchVehicles();
    setActionLoading(false);
  };

  const provisionKey = async (id: string) => {
    setActionLoading(true);
    setNewApiKey(null);
    setCopied(false);
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/v1/vehicles/${id}/api-key`,
      {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      }
    );
    if (res.ok) {
      const data = await res.json();
      setNewApiKey(data.plainTextKey);
    }
    await fetchVehicles();
    setActionLoading(false);
  };

  const revokeKey = async (id: string) => {
    if (!confirm("Revoke this vehicle's API key? The simulator will stop working until a new key is provisioned.")) return;
    setActionLoading(true);
    setNewApiKey(null);
    setCopied(false);
    await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/v1/vehicles/${id}/api-key`,
      {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      }
    );
    await fetchVehicles();
    setActionLoading(false);
  };

  const copyKey = () => {
    if (!newApiKey) return;
    navigator.clipboard.writeText(newApiKey);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const copyId = (id: string) => {
    navigator.clipboard.writeText(id);
    setCopiedId(true);
    setTimeout(() => setCopiedId(false), 2000);
  };

  return (
    <div className="flex h-full">
      <div className={`flex flex-col ${selectedVehicle ? "w-1/2" : "w-full"} transition-all`}>
        <div className="px-6 py-4 bg-white border-b border-gray-100">
          <h1 className="text-base font-medium text-gray-900">Fleet vehicles</h1>
          <p className="text-xs text-gray-400 mt-0.5">
            {vehicles.length} registered · click a row to manage
          </p>
        </div>

        <div className="flex-1 overflow-auto p-6">
          {loading && <div className="text-sm text-gray-400">Loading...</div>}
          {error && <div className="text-sm text-red-500">Error: {error}</div>}
          {vehicles.length === 0 && !loading && (
            <div className="flex items-center justify-center h-full text-sm text-gray-400">
              No vehicles registered
            </div>
          )}
          {vehicles.length > 0 && (
            <div className="bg-white border border-gray-100 rounded-xl overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100">
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Vehicle</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Type</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Status</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Battery</th>
                  </tr>
                </thead>
                <tbody>
                  {vehicles.map((v) => (
                    <tr
                      key={v.vehicleId}
                      onClick={() => {
                        setSelectedVehicle(v);
                        setNewApiKey(null);
                        setCopied(false);
                        setCopiedId(false);
                        setActiveTab("details");
                      }}
                      className={`border-b border-gray-50 last:border-0 cursor-pointer hover:bg-gray-50 transition-colors ${
                        selectedVehicle?.vehicleId === v.vehicleId ? "bg-gray-50" : ""
                      }`}
                    >
                      <td className="px-4 py-3">
                        <div className="font-medium text-gray-900">{v.modelName}</div>
                        <div className="text-xs text-gray-400">{v.licensePlate}</div>
                      </td>
                      <td className="px-4 py-3">
                        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${typeColor(v.type)}`}>
                          {v.type}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(v.status)}`}>
                          {v.status}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <div className="w-16 h-1.5 bg-gray-100 rounded-full overflow-hidden">
                            <div
                              className={`h-full rounded-full ${
                                v.batteryPercentage > 50 ? "bg-green-500" :
                                v.batteryPercentage > 20 ? "bg-yellow-500" : "bg-red-500"
                              }`}
                              style={{ width: `${v.batteryPercentage}%` }}
                            />
                          </div>
                          <span className="text-xs text-gray-500">{v.batteryPercentage}%</span>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {selectedVehicle && (
        <div className="w-1/2 border-l border-gray-100 bg-white flex flex-col">
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
            <span className="text-sm font-medium text-gray-900">
              {selectedVehicle.modelName} · {selectedVehicle.licensePlate}
            </span>
            <button
              onClick={() => { setSelectedVehicle(null); setNewApiKey(null); setCopied(false); setCopiedId(false); }}
              className="text-gray-400 hover:text-gray-600 text-lg leading-none"
            >
              ×
            </button>
          </div>

          <div className="flex border-b border-gray-100">
            {(["details", "maintenance"] as const).map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`px-5 py-2.5 text-xs font-medium transition-colors ${
                  activeTab === tab
                    ? "border-b-2 border-gray-900 text-gray-900"
                    : "text-gray-400 hover:text-gray-600"
                }`}
              >
                {tab.charAt(0).toUpperCase() + tab.slice(1)}
              </button>
            ))}
          </div>

          <div className="flex-1 overflow-auto p-6">
            {activeTab === "details" && (
              <div className="flex flex-col gap-5">
                <div className="grid grid-cols-2 gap-4">
                  <div className="col-span-2">
                    <div className="text-xs text-gray-400 mb-1">Vehicle ID</div>
                    <div className="flex items-center gap-2">
                      <div className="text-xs font-mono text-gray-700 truncate">
                        {selectedVehicle.vehicleId}
                      </div>
                      <button
                        onClick={() => copyId(selectedVehicle.vehicleId)}
                        className="text-xs px-2 py-0.5 rounded border border-gray-200 text-gray-500 hover:bg-gray-50 flex-shrink-0 transition-colors"
                      >
                        {copiedId ? "Copied!" : "Copy"}
                      </button>
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-400 mb-1">VIN</div>
                    <div className="text-xs font-mono text-gray-700">{selectedVehicle.vin}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-400 mb-1">Year</div>
                    <div className="text-sm text-gray-700">{selectedVehicle.yearOfManufacture}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-400 mb-1">Status</div>
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(selectedVehicle.status)}`}>
                      {selectedVehicle.status}
                    </span>
                  </div>
                  <div>
                    <div className="text-xs text-gray-400 mb-1">Battery</div>
                    <div className="text-sm text-gray-700">{selectedVehicle.batteryPercentage}%</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-400 mb-1">Location</div>
                    <div className="text-xs text-gray-500">
                      {selectedVehicle.locationLatitude.toFixed(4)}, {selectedVehicle.locationLongitude.toFixed(4)}
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-400 mb-1">Last inspection</div>
                    <div className="text-xs text-gray-500">
                      {selectedVehicle.lastInspectionDate
                        ? new Date(selectedVehicle.lastInspectionDate).toLocaleDateString()
                        : "Never"}
                    </div>
                  </div>
                </div>

                <div className="border-t border-gray-100 pt-4">
                  <div className="text-xs text-gray-400 mb-2">Status control</div>
                  <div className="flex gap-2">
                    {selectedVehicle.status !== "Active" && (
                      <button
                        onClick={() => activate(selectedVehicle.vehicleId)}
                        disabled={actionLoading}
                        className="text-xs px-3 py-1.5 rounded-lg bg-green-600 text-white hover:bg-green-700 disabled:opacity-50 transition-colors"
                      >
                        Activate
                      </button>
                    )}
                    {selectedVehicle.status === "Active" && (
                      <button
                        onClick={() => deactivate(selectedVehicle.vehicleId)}
                        disabled={actionLoading}
                        className="text-xs px-3 py-1.5 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-50 transition-colors"
                      >
                        Deactivate
                      </button>
                    )}
                  </div>
                </div>

                <div className="border-t border-gray-100 pt-4">
                  <div className="text-xs text-gray-400 mb-2">API key</div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => provisionKey(selectedVehicle.vehicleId)}
                      disabled={actionLoading}
                      className="text-xs px-3 py-1.5 rounded-lg border border-blue-200 text-blue-700 hover:bg-blue-50 disabled:opacity-50 transition-colors"
                    >
                      Provision new key
                    </button>
                    <button
                      onClick={() => revokeKey(selectedVehicle.vehicleId)}
                      disabled={actionLoading}
                      className="text-xs px-3 py-1.5 rounded-lg border border-red-200 text-red-600 hover:bg-red-50 disabled:opacity-50 transition-colors"
                    >
                      Revoke key
                    </button>
                  </div>

                  {newApiKey && (
                    <div className="mt-3 bg-yellow-50 border border-yellow-200 rounded-lg p-3">
                      <div className="text-xs font-medium text-yellow-800 mb-1">
                        New API key — copy now, won't be shown again
                      </div>
                      <div className="text-xs font-mono text-yellow-900 break-all mb-2">
                        {newApiKey}
                      </div>
                      <button
                        onClick={copyKey}
                        className="text-xs px-2 py-1 rounded border border-yellow-300 text-yellow-800 hover:bg-yellow-100 transition-colors"
                      >
                        {copied ? "Copied!" : "Copy to clipboard"}
                      </button>
                    </div>
                  )}
                </div>
              </div>
            )}

            {activeTab === "maintenance" && (
              <div>
                {loadingMaintenance && (
                  <div className="text-sm text-gray-400">Loading...</div>
                )}
                {!loadingMaintenance && maintenance.length === 0 && (
                  <div className="text-sm text-gray-400 text-center py-8">
                    No maintenance records
                  </div>
                )}
                {maintenance.length > 0 && (
                  <div className="flex flex-col gap-3">
                    {maintenance.map((log) => (
                      <div
                        key={log.logId}
                        className="bg-gray-50 rounded-lg p-3 border border-gray-100"
                      >
                        <div className="flex items-start justify-between gap-2">
                          <div className="text-sm font-medium text-gray-900">
                            {log.description}
                          </div>
                          <div className="text-sm font-medium text-gray-900 flex-shrink-0">
                            €{log.cost.toFixed(2)}
                          </div>
                        </div>
                        <div className="text-xs text-gray-400 mt-1.5 flex gap-3">
                          <span>{log.technicianName}</span>
                          <span>{new Date(log.serviceDate).toLocaleDateString()}</span>
                          <span>Next at {log.nextServiceMileage?.toLocaleString()} km</span>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}