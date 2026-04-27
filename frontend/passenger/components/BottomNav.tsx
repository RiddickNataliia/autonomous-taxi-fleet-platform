"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const tabs = [
  { href: "/home",    label: "Home",    icon: "🚗" },
  { href: "/rides",   label: "Rides",   icon: "🕐" },
  { href: "/profile", label: "Profile", icon: "👤" },
  { href: "/support", label: "Support", icon: "💬" },
];

export default function BottomNav() {
  const pathname = usePathname();

  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-100 px-2 pb-safe">
      <div className="flex max-w-md mx-auto">
        {tabs.map((tab) => {
          const active = pathname === tab.href || pathname.startsWith(tab.href + "/");
          return (
            <Link
              key={tab.href}
              href={tab.href}
              className={`flex-1 flex flex-col items-center py-3 gap-0.5 transition-colors ${
                active ? "text-gray-900" : "text-gray-400"
              }`}
            >
              <span className="text-lg">{tab.icon}</span>
              <span className="text-[10px] font-medium">{tab.label}</span>
            </Link>
          );
        })}
      </div>
    </nav>
  );
}