"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function CallbackPage() {
  const { isAuthenticated, isLoading, getAccessTokenSilently } = useAuth0();
  const router = useRouter();

  useEffect(() => {
    if (isAuthenticated) {
      getAccessTokenSilently().then((token) => {
        localStorage.setItem("passenger_token", token);
        router.push("/home");
      });
    }
  }, [isAuthenticated]);

  return (
    <div className="min-h-screen bg-white flex items-center justify-center">
      <div className="text-sm text-gray-400">
        {isLoading ? "Logging in..." : "Redirecting..."}
      </div>
    </div>
  );
}