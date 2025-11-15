import { apiClient } from "./client";
import { DashboardSummary } from "../types/dashboard";

export async function getDashboardSummary(): Promise<DashboardSummary> {
  return apiClient.get<DashboardSummary>("/api/dashboard/summary", {
    cache: "no-store",
  });
}
