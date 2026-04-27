"use client";

import { useState, useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { useRouter } from "next/navigation";
import BottomNav from "@/components/BottomNav";

interface Profile {
  passengerId: string;
  fullName: string;
  homeAddress: string;
  preferredPaymentMethod: string;
  loyaltyPoints: number;
}

const paymentMethods = ["CreditCard", "DebitCard", "PayPal", "Cash"];

export default function ProfilePage() {
  const { isAuthenticated, isLoading, logout } = useAuth0();
  const router = useRouter();
  const [profile, setProfile] = useState<Profile | null>(null);
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({
    fullName: "",
    homeAddress: "",
    preferredPaymentMethod: "CreditCard",
  });

  const token = typeof window !== "undefined"
    ? localStorage.getItem("passenger_token")
    : null;

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.push("/login");
  }, [isAuthenticated, isLoading]);

  const fetchProfile = async () => {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/v1/passengers/me`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    if (res.ok) {
      const data = await res.json();
      setProfile(data);
      setForm({
        fullName: data.fullName || "",
        homeAddress: data.homeAddress || "",
        preferredPaymentMethod: data.preferredPaymentMethod || "CreditCard",
      });
    }
  };

  useEffect(() => {
    fetchProfile();
  }, []);

  const saveProfile = async () => {
    setSaving(true);
    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/passengers/me`, {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(form),
    });
    await fetchProfile();
    setEditing(false);
    setSaving(false);
  };

  const handleLogout = () => {
    localStorage.removeItem("passenger_token");
    logout({ logoutParams: { returnTo: window.location.origin + "/login" } });
  };

  return (
    <div className="min-h-screen bg-white pb-20">
      <div className="max-w-md mx-auto px-4">
        <div className="pt-12 pb-6 flex items-center justify-between">
          <h1 className="text-xl font-semibold text-gray-900">Profile</h1>
          <button
            onClick={handleLogout}
            className="text-sm text-gray-400 hover:text-gray-600 transition-colors"
          >
            Logout
          </button>
        </div>

        {profile && (
          <div className="space-y-4">
            {/* Loyalty card */}
            <div className="bg-gray-900 text-white rounded-2xl p-5">
              <div className="text-xs text-gray-400 mb-1">Loyalty points</div>
              <div className="text-3xl font-semibold">{profile.loyaltyPoints}</div>
              <div className="text-xs text-gray-400 mt-1">
                {profile.loyaltyPoints >= 100
                  ? `€${Math.floor(profile.loyaltyPoints / 100).toFixed(0)} discount available`
                  : `${100 - (profile.loyaltyPoints % 100)} points to next €1 reward`}
              </div>
            </div>

            {/* Profile details */}
            <div className="bg-gray-50 rounded-2xl p-4 space-y-4">
              {editing ? (
                <>
                  <div>
                    <label className="block text-xs text-gray-400 mb-1">Full name</label>
                    <input
                      value={form.fullName}
                      onChange={(e) => setForm({ ...form, fullName: e.target.value })}
                      className="w-full bg-white border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-1 focus:ring-gray-300"
                    />
                  </div>
                  <div>
                    <label className="block text-xs text-gray-400 mb-1">Home address</label>
                    <input
                      value={form.homeAddress}
                      onChange={(e) => setForm({ ...form, homeAddress: e.target.value })}
                      className="w-full bg-white border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-1 focus:ring-gray-300"
                    />
                  </div>
                  <div>
                    <label className="block text-xs text-gray-400 mb-1">Payment method</label>
                    <select
                      value={form.preferredPaymentMethod}
                      onChange={(e) => setForm({ ...form, preferredPaymentMethod: e.target.value })}
                      className="w-full bg-white border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none"
                    >
                      {paymentMethods.map((m) => (
                        <option key={m} value={m}>{m}</option>
                      ))}
                    </select>
                  </div>
                  <div className="flex gap-2 pt-1">
                    <button
                      onClick={saveProfile}
                      disabled={saving}
                      className="flex-1 bg-gray-900 text-white text-sm py-2.5 rounded-xl disabled:opacity-50"
                    >
                      {saving ? "Saving..." : "Save"}
                    </button>
                    <button
                      onClick={() => setEditing(false)}
                      className="flex-1 border border-gray-200 text-gray-600 text-sm py-2.5 rounded-xl"
                    >
                      Cancel
                    </button>
                  </div>
                </>
              ) : (
                <>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-400">Name</span>
                    <span className="text-sm font-medium text-gray-900">
                      {profile.fullName || <span className="text-gray-400">Not set</span>}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-400">Home address</span>
                    <span className="text-sm text-gray-900 text-right max-w-[60%]">
                      {profile.homeAddress || <span className="text-gray-400">Not set</span>}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-400">Payment</span>
                    <span className="text-sm text-gray-900">
                      {profile.preferredPaymentMethod || "—"}
                    </span>
                  </div>
                  <button
                    onClick={() => setEditing(true)}
                    className="w-full border border-gray-200 text-gray-700 text-sm py-2.5 rounded-xl mt-2 hover:bg-gray-100 transition-colors"
                  >
                    Edit profile
                  </button>
                </>
              )}
            </div>
          </div>
        )}
      </div>
      <BottomNav />
    </div>
  );
}