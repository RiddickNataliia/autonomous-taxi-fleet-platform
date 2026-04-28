"use client";

import { useState, useEffect, useRef } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { useRouter } from "next/navigation";
import BottomNav from "@/components/BottomNav";

const vehicleTypes = [
  { type: null,       label: "Any",      multiplier: "fastest", emoji: "🚗" },
  { type: "Standard", label: "Standard", multiplier: "1.0×",    emoji: "🚗" },
  { type: "Van",      label: "Van",      multiplier: "1.5×",    emoji: "🚐" },
  { type: "Luxury",   label: "Luxury",   multiplier: "2.2×",    emoji: "🚙" },
];

interface NominatimResult {
  display_name: string;
  lat: string;
  lon: string;
}

interface AddressInputProps {
  value: string;
  onChange: (val: string) => void;
  onSelect: (val: string, lat: number, lon: number) => void;
  placeholder: string;
  dotColor: string;
}

function AddressInput({ value, onChange, onSelect, placeholder, dotColor }: AddressInputProps) {
  const [suggestions, setSuggestions] = useState<NominatimResult[]>([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const debounceRef = useRef<NodeJS.Timeout | null>(null);

  const handleChange = (val: string) => {
    onChange(val);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (val.length < 3) { setSuggestions([]); return; }

    debounceRef.current = setTimeout(async () => {
      try {
        const res = await fetch(
          `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(val)}&format=json&limit=5&countrycodes=be`,
          { headers: { "Accept-Language": "en" } }
        );
        const data = await res.json();
        setSuggestions(data);
        setShowSuggestions(true);
      } catch { }
    }, 400);
  };

  return (
    <div className="relative">
      <div className="flex items-center gap-3 bg-white rounded-xl px-4 py-3">
        <div className={`w-2 h-2 rounded-full flex-shrink-0 ${dotColor}`} />
        <input
          value={value}
          onChange={(e) => handleChange(e.target.value)}
          onFocus={() => suggestions.length > 0 && setShowSuggestions(true)}
          onBlur={() => setTimeout(() => setShowSuggestions(false), 200)}
          placeholder={placeholder}
          className="flex-1 text-sm text-gray-900 placeholder-gray-400 focus:outline-none bg-transparent"
        />
      </div>
      {showSuggestions && suggestions.length > 0 && (
        <div className="absolute left-0 right-0 top-full mt-1 bg-white border border-gray-100 rounded-xl shadow-lg z-50 overflow-hidden">
          {suggestions.map((s, i) => (
            <button
              key={i}
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => {
                const shortName = s.display_name.split(",")[0];
                onSelect(shortName, parseFloat(s.lat), parseFloat(s.lon));
                setSuggestions([]);
                setShowSuggestions(false);
              }}
              className="w-full text-left px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50 border-b border-gray-50 last:border-0"
            >
              <div className="font-medium truncate">{s.display_name.split(",")[0]}</div>
              <div className="text-xs text-gray-400 truncate">
                {s.display_name.split(",").slice(1, 3).join(",")}
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

export default function HomePage() {
  const { isAuthenticated, isLoading, getAccessTokenSilently } = useAuth0();
  const router = useRouter();

  const [departure, setDeparture]       = useState("");
  const [destination, setDestination]   = useState("");
  const [departureLat, setDepartureLat] = useState(51.0543);
  const [departureLon, setDepartureLon] = useState(3.7174);
  const [vehicleType, setVehicleType]   = useState<string | null>(null);
  const [discountCode, setDiscountCode] = useState("");
  const [showDiscount, setShowDiscount] = useState(false);
  const [booking, setBooking]           = useState(false);
  const [error, setError]               = useState("");
  const [profile, setProfile]           = useState<{ fullName: string; loyaltyPoints: number } | null>(null);

  // Discount code verification state
  const [codeStatus, setCodeStatus]   = useState<"idle" | "checking" | "valid" | "invalid">("idle");
  const [codeMessage, setCodeMessage] = useState("");

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.push("/login");
  }, [isAuthenticated, isLoading]);

  useEffect(() => {
    if (isAuthenticated) {
      getAccessTokenSilently().then((token) => {
        localStorage.setItem("passenger_token", token);
        fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/passengers/me`, {
          method: "POST",
          headers: { Authorization: `Bearer ${token}` },
        }).then(() => {
          fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/passengers/me`, {
            headers: { Authorization: `Bearer ${token}` },
          })
            .then((r) => {
              if (!r.ok) return null;
              return r.json();
            })
            .then((d) => {
              if (d) setProfile({ fullName: d.fullName, loyaltyPoints: d.loyaltyPoints });
            })
            .catch(() => {});
        });
      });
    }
  }, [isAuthenticated]);

  const verifyCode = async () => {
    if (!discountCode) return;
    setCodeStatus("checking");
    setCodeMessage("");
    try {
      const token = localStorage.getItem("passenger_token");
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/v1/discounts/${discountCode}`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      if (!res.ok) {
        setCodeStatus("invalid");
        setCodeMessage("Code not found.");
        return;
      }
      const data = await res.json();
      if (!data.isActive) {
        setCodeStatus("invalid");
        setCodeMessage("This code is inactive.");
        return;
      }
      if (new Date(data.expirationDate) < new Date()) {
        setCodeStatus("invalid");
        setCodeMessage("This code has expired.");
        return;
      }
      setCodeStatus("valid");
      setCodeMessage(
        data.type === "Percentage"
          ? `${data.value}% off · min. ride value €${data.minimumRideValue.toFixed(2)}`
          : `€${data.value.toFixed(2)} off · min. ride value €${data.minimumRideValue.toFixed(2)}`
      );
    } catch {
      setCodeStatus("invalid");
      setCodeMessage("Could not verify code.");
    }
  };

  const requestRide = async () => {
    if (!departure || !destination) {
      setError("Please enter departure and destination.");
      return;
    }
    setError("");
    setBooking(true);
    try {
      const token = localStorage.getItem("passenger_token");
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/v1/rides`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          departure,
          destination,
          estimatedDistanceKm: 10.0,
          passengerLatitude: departureLat,
          passengerLongitude: departureLon,
          preferredVehicleType: vehicleType,
          discountCode: discountCode || null,
        }),
      });

      if (!res.ok) {
        const data = await res.json().catch(() => null);
        const msg = (data?.detail || data?.message || data?.title || "").toLowerCase();
        if (msg.includes("no available")) {
          setError(
            vehicleType && vehicleType !== "Any"
              ? `No ${vehicleType} vehicles available nearby. Try selecting "Any" or a different type.`
              : "No vehicles available nearby right now. Please try again in a few minutes."
          );
        } else if (msg.includes("active ride") || msg.includes("already have")) {
          setError("You already have an active ride. Please complete or cancel it before booking a new one.");
        } else if (msg.includes("invalid or has expired") || msg.includes("discount")) {
          setError("Discount code is invalid or has expired.");
        } else {
          setError("Could not book ride. Please try again.");
        }
        return;
      }

      router.push("/rides");
    } catch {
      setError("Could not connect to the server. Make sure the API is running.");
    } finally {
      setBooking(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-sm text-gray-400">Loading...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-white pb-20">
      <div className="max-w-md mx-auto px-4">
        <div className="pt-12 pb-6">
          <div className="text-xl font-semibold text-gray-900">
            {profile?.fullName ? `Hi, ${profile.fullName.split(" ")[0]}` : "Nova Drive"}
          </div>
          <div className="text-sm text-gray-400 mt-0.5">Where are you going?</div>
          {profile && (
            <div className="mt-2 inline-flex items-center gap-1.5 bg-gray-50 px-3 py-1 rounded-full">
              <span className="text-xs text-gray-500">⭐ {profile.loyaltyPoints} loyalty points</span>
            </div>
          )}
        </div>

        <div className="bg-gray-50 rounded-2xl p-4 space-y-3">
          <div className="space-y-2">
            <AddressInput
              value={departure}
              onChange={setDeparture}
              onSelect={(val, lat, lon) => {
                setDeparture(val);
                setDepartureLat(lat);
                setDepartureLon(lon);
              }}
              placeholder="Departure location"
              dotColor="bg-green-500"
            />
            <AddressInput
              value={destination}
              onChange={setDestination}
              onSelect={(val, lat, lon) => {
                setDestination(val);
              }}
              placeholder="Destination"
              dotColor="bg-red-500"
            />
          </div>

          <div className="grid grid-cols-4 gap-2">
            {vehicleTypes.map((v) => (
              <button
                key={v.label}
                onClick={() => setVehicleType(v.type)}
                className={`flex flex-col items-center py-2.5 px-1 rounded-xl border transition-colors ${
                  vehicleType === v.type
                    ? "border-gray-900 bg-gray-900 text-white"
                    : "border-gray-200 bg-white text-gray-600"
                }`}
              >
                <span className="text-lg">{v.emoji}</span>
                <span className="text-[10px] font-medium mt-1">{v.label}</span>
                <span className={`text-[9px] mt-0.5 ${vehicleType === v.type ? "text-gray-300" : "text-gray-400"}`}>
                  {v.multiplier}
                </span>
              </button>
            ))}
          </div>

          {/* Discount code section */}
          <div>
            <button
              onClick={() => {
                setShowDiscount(!showDiscount);
                if (showDiscount) {
                  setDiscountCode("");
                  setCodeStatus("idle");
                  setCodeMessage("");
                }
              }}
              className="text-xs text-gray-400 hover:text-gray-600 transition-colors"
            >
              {showDiscount ? "Hide" : "+ Add discount code"}
            </button>

            {showDiscount && (
              <div className="mt-2 flex flex-col gap-2">
                <div className="flex gap-2">
                  <input
                    value={discountCode}
                    onChange={(e) => {
                      setDiscountCode(e.target.value.toUpperCase());
                      setCodeStatus("idle");
                      setCodeMessage("");
                    }}
                    onKeyDown={(e) => e.key === "Enter" && verifyCode()}
                    placeholder="e.g. SUMMER24"
                    className="flex-1 bg-white border border-gray-200 rounded-xl px-4 py-2.5 text-sm font-mono focus:outline-none focus:ring-1 focus:ring-gray-300"
                  />
                  <button
                    onClick={verifyCode}
                    disabled={!discountCode || codeStatus === "checking"}
                    className="text-xs px-4 py-2 rounded-xl border border-gray-200 bg-white text-gray-600 hover:bg-gray-50 disabled:opacity-40 transition-colors flex-shrink-0 font-medium"
                  >
                    {codeStatus === "checking" ? "..." : "Verify"}
                  </button>
                </div>

                {codeMessage && (
                  <p className={`text-xs px-1 ${codeStatus === "valid" ? "text-green-600" : "text-red-500"}`}>
                    {codeStatus === "valid" ? "✓ " : "✗ "}{codeMessage}
                  </p>
                )}
              </div>
            )}
          </div>

          {error && (
            <div className="bg-red-50 border border-red-100 rounded-xl px-4 py-3">
              <p className="text-xs text-red-600">{error}</p>
            </div>
          )}

          <button
            onClick={requestRide}
            disabled={booking}
            className="w-full bg-gray-900 text-white py-3.5 rounded-xl text-sm font-medium hover:bg-gray-800 disabled:opacity-50 transition-colors"
          >
            {booking ? "Booking..." : "Request ride"}
          </button>
        </div>
      </div>
      <BottomNav />
    </div>
  );
}
