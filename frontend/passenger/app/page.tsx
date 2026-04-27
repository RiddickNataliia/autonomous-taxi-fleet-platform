"use client";

import { useAuth0 } from "@auth0/auth0-react";
import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function Home() {
  const { isAuthenticated, isLoading } = useAuth0();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading) {
      if (isAuthenticated) {
        router.push("/home");
      } else {
        router.push("/login");
      }
    }
  }, [isLoading, isAuthenticated]);

  return (
    <div className="min-h-screen bg-white flex items-center justify-center">
      <div className="text-sm text-gray-400">Loading...</div>
    </div>
  );
}