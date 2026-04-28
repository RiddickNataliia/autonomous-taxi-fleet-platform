"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function LoginPage() {
  const { loginWithRedirect, isAuthenticated, isLoading, getAccessTokenSilently, error } = useAuth0();
  const router = useRouter();

  useEffect(() => {
    if (isAuthenticated) {
      getAccessTokenSilently().then((token) => {
        localStorage.setItem("admin_token", token);
        router.push("/dashboard");
      });
    }
  }, [isAuthenticated]);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-sm text-gray-400">Loading...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center">
      <div className="bg-white border border-gray-100 rounded-xl p-8 w-full max-w-sm shadow-sm">
        <div className="mb-6">
          <h1 className="text-lg font-medium text-gray-900">Nova Drive</h1>
          <p className="text-sm text-gray-400 mt-1">Admin Console</p>
        </div>
        {error && (
            <div className="mb-4 text-xs text-red-500 bg-red-50 rounded-lg px-3 py-2">
              {error.message}
            </div>
          )}
          <button
            onClick={() => loginWithRedirect()}
            className="w-full bg-gray-900 text-white text-sm py-2.5 rounded-lg hover:bg-gray-800 transition-colors"
          >
            Login with Auth0
          </button>
      </div>
    </div>
  );
}