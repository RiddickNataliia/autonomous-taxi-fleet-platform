import type { Metadata } from "next";
import { Geist } from "next/font/google";
import "./globals.css";
import { Providers } from "@/lib/apollo-provider";

const geist = Geist({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "NovaDrive Admin",
  description: "NovaDrive Fleet Management Console",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className={geist.className}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}