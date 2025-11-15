import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Nav } from "@/components/layout/nav";

const inter = Inter({
  subsets: ["latin"],
  variable: "--font-inter",
});

export const metadata: Metadata = {
  title: "HOB Dashboard - House of Burgesses Services",
  description: "Dashboard for managing customers, orders, and sales",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={`${inter.variable} font-sans antialiased bg-gray-50`}>
        <Nav />
        <main className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-8">
          {children}
        </main>
      </body>
    </html>
  );
}
