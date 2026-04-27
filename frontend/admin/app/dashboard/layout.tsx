"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useAuth0 } from "@auth0/auth0-react";

const navItems = [
  { href: "/dashboard",             label: "Fleet map",   section: "Fleet" },
  { href: "/dashboard/vehicles",    label: "Vehicles",    section: "Fleet" },
  { href: "/dashboard/telemetry",   label: "Telemetry",   section: "Fleet" },
  { href: "/dashboard/diagnostics", label: "Diagnostics", section: "Fleet" },
  { href: "/dashboard/rides",       label: "Rides",       section: "Operations" },
  { href: "/dashboard/passengers",  label: "Passengers",  section: "Operations" },
  { href: "/dashboard/support",     label: "Support",     section: "Operations" },
  { href: "/dashboard/discounts",   label: "Discounts",   section: "Operations" },
];

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const router = useRouter();
  const { logout, user } = useAuth0();
  const sections = [...new Set(navItems.map((i) => i.section))];

  const handleLogout = () => {
    localStorage.removeItem("admin_token");
    logout({ logoutParams: { returnTo: window.location.origin + "/login" } });
  };

  return (
    <div className="flex h-screen bg-gray-50 overflow-hidden">
      <aside className="w-52 bg-white border-r border-gray-100 flex flex-col flex-shrink-0">
        <div className="px-4 py-4 border-b border-gray-100">
          <div className="text-sm font-medium text-gray-900 tracking-tight">
            Nova Drive
          </div>
          <div className="text-xs text-gray-400 mt-0.5">Admin Console</div>
        </div>

        <nav className="flex-1 py-2 overflow-y-auto">
          {sections.map((section) => (
            <div key={section}>
              <div className="px-4 py-2 text-[10px] uppercase tracking-widest text-gray-400 font-medium">
                {section}
              </div>
              {navItems
                .filter((i) => i.section === section)
                .map((item) => {
                  const active = pathname === item.href;
                  return (
                    <Link
                      key={item.href}
                      href={item.href}
                      className={`flex items-center px-4 py-2 text-sm transition-colors ${
                        active
                          ? "bg-gray-50 text-gray-900 font-medium"
                          : "text-gray-500 hover:text-gray-900 hover:bg-gray-50"
                      }`}
                    >
                      {item.label}
                    </Link>
                  );
                })}
            </div>
          ))}
        </nav>

        <div className="border-t border-gray-100 p-3 flex flex-col gap-2">
          {user && (
            <div className="px-1">
              <div className="text-xs font-medium text-gray-700 truncate">{user.email}</div>
              <div className="text-xs text-gray-400 mt-0.5">Administrator</div>
            </div>
          )}
          <div className="flex items-center justify-between">
            <div className="text-xs text-gray-400">
              <span className="inline-block w-1.5 h-1.5 rounded-full bg-green-500 mr-1.5 mb-0.5"></span>
              Simulator running
            </div>
            <button
              onClick={handleLogout}
              className="text-xs text-gray-400 hover:text-gray-600 transition-colors"
            >
              Logout
            </button>
          </div>
        </div>
      </aside>

      <main className="flex-1 overflow-auto">{children}</main>
    </div>
  );
}