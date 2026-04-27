"use client";

import { useState, useEffect } from "react";

interface SupportTicket {
  ticketId: string;
  subject: string;
  description: string;
  priority: string;
  status: string;
  createdAt: string;
  passengerId: string;
}

const priorityColor = (p: string) => {
  switch (p) {
    case "Critical": return "bg-red-100 text-red-700";
    case "High":     return "bg-orange-100 text-orange-700";
    case "Medium":   return "bg-yellow-100 text-yellow-700";
    default:         return "bg-gray-100 text-gray-500";
  }
};

const statusColor = (s: string) => {
  switch (s) {
    case "Open":       return "bg-blue-50 text-blue-700";
    case "InProgress": return "bg-yellow-50 text-yellow-700";
    case "Resolved":   return "bg-green-50 text-green-700";
    default:           return "bg-gray-100 text-gray-500";
  }
};

export default function SupportPage() {
  const [tickets, setTickets] = useState<SupportTicket[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedTicket, setSelectedTicket] = useState<SupportTicket | null>(null);
  const [updatingId, setUpdatingId] = useState<string | null>(null);

  const token = typeof window !== "undefined"
    ? localStorage.getItem("admin_token")
    : null;

  const fetchTickets = async () => {
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/v1/support`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      setTickets(data);
      if (selectedTicket) {
        const updated = data.find((t: SupportTicket) => t.ticketId === selectedTicket.ticketId);
        if (updated) setSelectedTicket(updated);
      }
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTickets();
    const interval = setInterval(fetchTickets, 15000);
    return () => clearInterval(interval);
  }, []);

  const startTicket = async (id: string) => {
    setUpdatingId(id);
    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/support/${id}/start`, {
      method: "PUT",
      headers: { Authorization: `Bearer ${token}` },
    });
    await fetchTickets();
    setUpdatingId(null);
  };

  const resolveTicket = async (id: string) => {
    setUpdatingId(id);
    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/support/${id}/resolve`, {
      method: "PUT",
      headers: { Authorization: `Bearer ${token}` },
    });
    await fetchTickets();
    setUpdatingId(null);
  };

  const updatePriority = async (id: string, priority: string) => {
    setUpdatingId(id);
    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/support/${id}/priority`, {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ priority }),
    });
    await fetchTickets();
    setUpdatingId(null);
  };

  return (
    <div className="flex h-full">
      {/* Main list */}
      <div className={`flex flex-col ${selectedTicket ? "w-1/2" : "w-full"} transition-all`}>
        <div className="px-6 py-4 bg-white border-b border-gray-100">
          <h1 className="text-base font-medium text-gray-900">Support tickets</h1>
          <p className="text-xs text-gray-400 mt-0.5">{tickets.length} total · click a row to open</p>
        </div>

        <div className="flex-1 overflow-auto p-6">
          {loading && <div className="text-sm text-gray-400">Loading...</div>}
          {error && <div className="text-sm text-red-500">Error: {error}</div>}
          {!loading && tickets.length === 0 && !error && (
            <div className="flex items-center justify-center h-full text-sm text-gray-400">
              No support tickets yet
            </div>
          )}
          {tickets.length > 0 && (
            <div className="bg-white border border-gray-100 rounded-xl overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100">
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Subject</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Priority</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Status</th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-400">Reported</th>
                  </tr>
                </thead>
                <tbody>
                  {tickets.map((t) => (
                    <tr
                      key={t.ticketId}
                      onClick={() => setSelectedTicket(t)}
                      className={`border-b border-gray-50 last:border-0 cursor-pointer hover:bg-gray-50 transition-colors ${
                        selectedTicket?.ticketId === t.ticketId ? "bg-gray-50" : ""
                      }`}
                    >
                      <td className="px-4 py-3">
                        <div className="font-medium text-gray-900">{t.subject}</div>
                        <div className="text-xs text-gray-400 mt-0.5 max-w-xs truncate">
                          {t.description}
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${priorityColor(t.priority)}`}>
                          {t.priority}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(t.status)}`}>
                          {t.status}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-xs text-gray-400">
                        {new Date(t.createdAt).toLocaleString()}
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
      {selectedTicket && (
        <div className="w-1/2 border-l border-gray-100 bg-white flex flex-col">
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
            <span className="text-sm font-medium text-gray-900">Ticket details</span>
            <button
              onClick={() => setSelectedTicket(null)}
              className="text-gray-400 hover:text-gray-600 text-lg leading-none"
            >
              ×
            </button>
          </div>

          <div className="flex-1 overflow-auto p-6 flex flex-col gap-5">
            <div>
              <div className="text-xs text-gray-400 mb-1">Subject</div>
              <div className="text-sm font-medium text-gray-900">{selectedTicket.subject}</div>
            </div>

            <div>
              <div className="text-xs text-gray-400 mb-1">Description</div>
              <div className="text-sm text-gray-700 leading-relaxed whitespace-pre-wrap bg-gray-50 rounded-lg p-3">
                {selectedTicket.description}
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="text-xs text-gray-400 mb-1">Status</div>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColor(selectedTicket.status)}`}>
                  {selectedTicket.status}
                </span>
              </div>
              <div>
                <div className="text-xs text-gray-400 mb-1">Reported</div>
                <div className="text-sm text-gray-700">
                  {new Date(selectedTicket.createdAt).toLocaleString()}
                </div>
              </div>
              <div>
                <div className="text-xs text-gray-400 mb-2">Priority</div>
                <select
                  value={selectedTicket.priority}
                  disabled={selectedTicket.status === "Resolved" || updatingId === selectedTicket.ticketId}
                  onChange={(e) => updatePriority(selectedTicket.ticketId, e.target.value)}
                  className="text-sm border border-gray-200 rounded-lg px-2 py-1.5 focus:outline-none w-full"
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </select>
              </div>
              <div>
                <div className="text-xs text-gray-400 mb-1">Passenger ID</div>
                <div className="text-xs text-gray-500 font-mono truncate">
                  {selectedTicket.passengerId}
                </div>
              </div>
            </div>

            <div className="border-t border-gray-100 pt-4 flex flex-col gap-2">
              <div className="text-xs text-gray-400 mb-1">Actions</div>
              {selectedTicket.status === "Open" && (
                <button
                  onClick={() => startTicket(selectedTicket.ticketId)}
                  disabled={updatingId === selectedTicket.ticketId}
                  className="text-sm px-4 py-2 rounded-lg border border-gray-200 text-gray-700 hover:bg-gray-50 disabled:opacity-50 transition-colors"
                >
                  Start working on this ticket
                </button>
              )}
              {selectedTicket.status === "InProgress" && (
                <button
                  onClick={() => resolveTicket(selectedTicket.ticketId)}
                  disabled={updatingId === selectedTicket.ticketId}
                  className="text-sm px-4 py-2 rounded-lg bg-green-600 text-white hover:bg-green-700 disabled:opacity-50 transition-colors"
                >
                  Mark as resolved
                </button>
              )}
              {selectedTicket.status === "Resolved" && (
                <div className="text-sm text-gray-400 text-center py-2">
                  This ticket has been resolved.
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}