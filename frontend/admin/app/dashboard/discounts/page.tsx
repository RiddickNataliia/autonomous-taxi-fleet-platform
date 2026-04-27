"use client";

import { useState, useEffect } from "react";

interface DiscountCode {
  id: string;
  code: string;
  type: string;
  value: number;
  minimumRideValue: number;
  expirationDate: string;
  isActive: boolean;
}

export default function DiscountsPage() {
  const [discounts, setDiscounts] = useState<DiscountCode[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [reactivatingId, setReactivatingId] = useState<string | null>(null);
  const [newExpiry, setNewExpiry] = useState<{ [id: string]: string }>({});
  const [form, setForm] = useState({
    code: "",
    type: "Percentage",
    value: "",
    minimumRideValue: "10",
    expirationDate: "",
  });

  const token = typeof window !== "undefined"
    ? localStorage.getItem("admin_token")
    : null;

  const fetchDiscounts = async () => {
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/v1/discounts`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      setDiscounts(data);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDiscounts();
  }, []);

  const createDiscount = async () => {
    setSubmitting(true);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/v1/discounts`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            code: form.code.toUpperCase(),
            type: form.type,
            value: parseFloat(form.value),
            minimumRideValue: parseFloat(form.minimumRideValue),
            expirationDate: new Date(form.expirationDate).toISOString(),
          }),
        }
      );
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      setShowForm(false);
      setForm({ code: "", type: "Percentage", value: "", minimumRideValue: "10", expirationDate: "" });
      await fetchDiscounts();
    } catch (e: any) {
      setError(e.message);
    } finally {
      setSubmitting(false);
    }
  };

  const deleteDiscount = async (id: string) => {
    if (!confirm("Permanently delete this discount code?")) return;
    await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/v1/discounts/${id}`,
      {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      }
    );
    await fetchDiscounts();
  };

  const deactivateDiscount = async (id: string) => {
    await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/v1/discounts/${id}/deactivate`,
      {
        method: "PUT",
        headers: { Authorization: `Bearer ${token}` },
      }
    );
    await fetchDiscounts();
  };

  const reactivateDiscount = async (id: string) => {
    const expiry = newExpiry[id];
    if (!expiry) {
      alert("Please select a new expiration date first.");
      return;
    }
    setReactivatingId(id);
    await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/v1/discounts/${id}/activate`,
      {
        method: "PUT",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          newExpirationDate: new Date(expiry).toISOString(),
        }),
      }
    );
    setNewExpiry((prev) => { const n = { ...prev }; delete n[id]; return n; });
    setReactivatingId(null);
    await fetchDiscounts();
  };

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between px-6 py-4 bg-white border-b border-gray-100">
        <div>
          <h1 className="text-base font-medium text-gray-900">Discount codes</h1>
          <p className="text-xs text-gray-400 mt-0.5">{discounts.length} total</p>
        </div>
        <button
          onClick={() => setShowForm(!showForm)}
          className="text-sm bg-gray-900 text-white px-3 py-1.5 rounded-lg hover:bg-gray-800 transition-colors"
        >
          {showForm ? "Cancel" : "+ New code"}
        </button>
      </div>

      <div className="p-6 flex-1 overflow-auto">
        {error && <div className="text-sm text-red-500 mb-4">Error: {error}</div>}

        {showForm && (
          <div className="bg-white border border-gray-100 rounded-xl p-5 mb-4">
            <div className="text-sm font-medium text-gray-900 mb-4">Create discount code</div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs text-gray-400 mb-1">Code</label>
                <input
                  value={form.code}
                  onChange={(e) => setForm({ ...form, code: e.target.value.toUpperCase() })}
                  placeholder="SUMMER24"
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-300 font-mono"
                />
              </div>
              <div>
                <label className="block text-xs text-gray-400 mb-1">Type</label>
                <select
                  value={form.type}
                  onChange={(e) => setForm({ ...form, type: e.target.value })}
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none"
                >
                  <option value="Percentage">Percentage (%)</option>
                  <option value="Flat">Flat (€)</option>
                </select>
              </div>
              <div>
                <label className="block text-xs text-gray-400 mb-1">
                  Value ({form.type === "Percentage" ? "%" : "€"})
                </label>
                <input
                  type="number"
                  value={form.value}
                  onChange={(e) => setForm({ ...form, value: e.target.value })}
                  placeholder={form.type === "Percentage" ? "15" : "5.00"}
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none"
                />
              </div>
              <div>
                <label className="block text-xs text-gray-400 mb-1">Minimum ride value (€)</label>
                <input
                  type="number"
                  value={form.minimumRideValue}
                  onChange={(e) => setForm({ ...form, minimumRideValue: e.target.value })}
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none"
                />
              </div>
              <div className="col-span-2">
                <label className="block text-xs text-gray-400 mb-1">Expiration date</label>
                <input
                  type="date"
                  value={form.expirationDate}
                  onChange={(e) => setForm({ ...form, expirationDate: e.target.value })}
                  className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none"
                />
              </div>
            </div>
            <button
              onClick={createDiscount}
              disabled={submitting || !form.code || !form.value || !form.expirationDate}
              className="mt-4 text-sm bg-gray-900 text-white px-4 py-2 rounded-lg hover:bg-gray-800 disabled:opacity-50 transition-colors"
            >
              {submitting ? "Creating..." : "Create code"}
            </button>
          </div>
        )}

        {loading && <div className="text-sm text-gray-400">Loading...</div>}
        {!loading && discounts.length === 0 && !showForm && (
          <div className="flex items-center justify-center h-full text-sm text-gray-400">
            No discount codes yet
          </div>
        )}
        {discounts.length > 0 && (
          <div className="bg-white border border-gray-100 rounded-xl overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100">
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Code</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Type</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Value</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Min. ride</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Expires</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Status</th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Actions</th>
                </tr>
              </thead>
              <tbody>
                {discounts.map((d) => (
                  <tr key={d.id} className="border-b border-gray-50 last:border-0">
                    <td className="px-4 py-3 font-mono font-medium text-gray-900">{d.code}</td>
                    <td className="px-4 py-3 text-gray-500">{d.type}</td>
                    <td className="px-4 py-3 text-gray-900">
                      {d.type === "Percentage" ? `${d.value}%` : `€${d.value.toFixed(2)}`}
                    </td>
                    <td className="px-4 py-3 text-gray-500">€{d.minimumRideValue?.toFixed(2)}</td>
                    <td className="px-4 py-3 text-xs text-gray-400">
                      {new Date(d.expirationDate).toLocaleDateString()}
                    </td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                        d.isActive
                          ? "bg-green-50 text-green-700"
                          : "bg-gray-100 text-gray-400"
                      }`}>
                        {d.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex flex-col gap-1.5">
                        {d.isActive && (
                          <button
                            onClick={() => deactivateDiscount(d.id)}
                            className="text-xs px-2 py-1 rounded border border-yellow-200 text-yellow-700 hover:bg-yellow-50 transition-colors"
                          >
                            Deactivate
                          </button>
                        )}
                        {!d.isActive && (
                          <div className="flex flex-col gap-1">
                            <input
                              type="date"
                              value={newExpiry[d.id] ?? ""}
                              onChange={(e) => setNewExpiry((prev) => ({ ...prev, [d.id]: e.target.value }))}
                              className="text-xs border border-gray-200 rounded px-1.5 py-0.5 focus:outline-none"
                            />
                            <button
                              onClick={() => reactivateDiscount(d.id)}
                              disabled={reactivatingId === d.id || !newExpiry[d.id]}
                              className="text-xs px-2 py-1 rounded border border-green-200 text-green-700 hover:bg-green-50 disabled:opacity-50 transition-colors"
                            >
                              Reactivate
                            </button>
                          </div>
                        )}
                        <button
                          onClick={() => deleteDiscount(d.id)}
                          className="text-xs px-2 py-1 rounded border border-red-200 text-red-600 hover:bg-red-50 transition-colors"
                        >
                          Delete
                        </button>
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
  );
}