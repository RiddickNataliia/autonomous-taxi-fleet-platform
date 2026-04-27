"use client";

import { useState, useEffect, Suspense } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { useRouter, useSearchParams } from "next/navigation";
import BottomNav from "@/components/BottomNav";

interface Ticket {
  ticketId: string;
  subject: string;
  description: string;
  priority: string;
  status: string;
  createdAt: string;
}

const statusColor = (s: string) => {
  switch (s) {
    case "Open":       return "text-blue-600 bg-blue-50";
    case "InProgress": return "text-yellow-600 bg-yellow-50";
    case "Resolved":   return "text-green-600 bg-green-50";
    default:           return "text-gray-500 bg-gray-100";
  }
};

function SupportContent() {
  const { isAuthenticated, isLoading } = useAuth0();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({ subject: "", description: "" });

  const token = typeof window !== "undefined"
    ? localStorage.getItem("passenger_token")
    : null;

  // Pre-fill form if coming from ride detail
  useEffect(() => {
    const rideId = searchParams.get("rideId");
    const route = searchParams.get("route");
    if (rideId && route) {
      setShowForm(true);
      setForm({
        subject: `Issue with ride: ${route}`,
        description: `Ride ID: ${rideId}\nRoute: ${route}\n\nPlease describe your issue:\n`,
      });
    }
  }, [searchParams]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.push("/login");
  }, [isAuthenticated, isLoading]);

  const fetchTickets = async () => {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/v1/support/my`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    if (res.ok) setTickets(await res.json());
  };

  useEffect(() => {
    fetchTickets();
  }, []);

  const createTicket = async () => {
    if (!form.subject || !form.description) return;
    setSubmitting(true);
    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/support`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(form),
    });
    setForm({ subject: "", description: "" });
    setShowForm(false);
    await fetchTickets();
    setSubmitting(false);
  };

  return (
    <div className="min-h-screen bg-white pb-20">
      <div className="max-w-md mx-auto px-4">
        <div className="pt-12 pb-6 flex items-center justify-between">
          <h1 className="text-xl font-semibold text-gray-900">Support</h1>
          <button
            onClick={() => setShowForm(!showForm)}
            className="text-sm bg-gray-900 text-white px-3 py-1.5 rounded-xl"
          >
            {showForm ? "Cancel" : "+ New ticket"}
          </button>
        </div>

        {showForm && (
          <div className="bg-gray-50 rounded-2xl p-4 mb-4 space-y-3">
            <div>
              <label className="block text-xs text-gray-400 mb-1">Subject</label>
              <input
                value={form.subject}
                onChange={(e) => setForm({ ...form, subject: e.target.value })}
                placeholder="What's the issue?"
                className="w-full bg-white border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none"
              />
            </div>
            <div>
              <label className="block text-xs text-gray-400 mb-1">Description</label>
              <textarea
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                placeholder="Describe the problem in detail..."
                rows={5}
                className="w-full bg-white border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none resize-none"
              />
            </div>
            <button
              onClick={createTicket}
              disabled={submitting || !form.subject || !form.description}
              className="w-full bg-gray-900 text-white text-sm py-3 rounded-xl disabled:opacity-50"
            >
              {submitting ? "Submitting..." : "Submit ticket"}
            </button>
          </div>
        )}

        {tickets.length === 0 && !showForm && (
          <div className="text-center py-16">
            <div className="text-4xl mb-3">💬</div>
            <div className="text-sm text-gray-400">No support tickets</div>
            <button
              onClick={() => setShowForm(true)}
              className="mt-4 text-sm text-gray-900 font-medium underline"
            >
              Create one
            </button>
          </div>
        )}

        <div className="space-y-2">
          {tickets.map((t) => (
            <div key={t.ticketId} className="bg-gray-50 rounded-2xl p-4">
              <div className="flex items-start justify-between gap-2">
                <div className="min-w-0">
                  <div className="text-sm font-medium text-gray-900">{t.subject}</div>
                  <div className="text-xs text-gray-400 mt-0.5 line-clamp-2">
                    {t.description}
                  </div>
                  <div className="text-xs text-gray-400 mt-1">
                    {new Date(t.createdAt).toLocaleDateString()}
                  </div>
                </div>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium flex-shrink-0 ${statusColor(t.status)}`}>
                  {t.status}
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>
      <BottomNav />
    </div>
  );
}

export default function SupportPage() {
  return (
    <Suspense fallback={<div className="min-h-screen bg-white flex items-center justify-center"><div className="text-sm text-gray-400">Loading...</div></div>}>
      <SupportContent />
    </Suspense>
  );
}