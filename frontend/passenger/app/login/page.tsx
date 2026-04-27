"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function LoginPage() {
  const { loginWithRedirect, isAuthenticated, isLoading, getAccessTokenSilently } = useAuth0();
  const router = useRouter();

  useEffect(() => {
    if (isAuthenticated) {
      getAccessTokenSilently().then((token) => {
        localStorage.setItem("passenger_token", token);
        router.push("/home");
      });
    }
  }, [isAuthenticated]);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-sm text-gray-400">Loading...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-white flex flex-col">
      <div className="flex-1 flex flex-col items-center justify-center px-6">
        <div className="w-full max-w-sm">
          <div className="mb-10 text-center">
            <div className="text-2xl font-semibold text-gray-900 tracking-tight">
              Nova Drive
            </div>
            <div className="text-sm text-gray-400 mt-1">
              Your autonomous ride
            </div>
          </div>

          <div className="space-y-3">
            <button
              onClick={() => loginWithRedirect()}
              className="w-full bg-gray-900 text-white text-sm py-3 rounded-xl hover:bg-gray-800 transition-colors font-medium"
            >
              Continue with Auth0
            </button>
          </div>

          <p className="text-xs text-gray-400 text-center mt-6">
            By continuing you agree to our Terms of Service
          </p>
        </div>
      </div>
    </div>
  );
}