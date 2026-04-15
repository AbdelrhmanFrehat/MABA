import type {
  ControlCenterJobDetail,
  ControlCenterJobListItem,
} from "../models/job.types";

type JobFilters = {
  status?: string;
  machineType?: string;
};

const apiBaseUrl =
  (
    globalThis.localStorage?.getItem("maba.apiBaseUrl") ??
    (globalThis as { mabaApiBaseUrl?: string }).mabaApiBaseUrl ??
    "http://localhost:5000"
  ).replace(/\/$/, "");

function buildUrl(path: string, filters?: JobFilters): string {
  const url = new URL(`${apiBaseUrl}${path}`);

  if (filters?.status) {
    url.searchParams.set("status", filters.status);
  }

  if (filters?.machineType) {
    url.searchParams.set("machineType", filters.machineType);
  }

  return url.toString();
}

async function readJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with status ${response.status}`);
  }

  return (await response.json()) as T;
}

export async function fetchJobs(
  filters?: JobFilters
): Promise<ControlCenterJobListItem[]> {
  const response = await fetch(buildUrl("/api/v1/control-center/jobs", filters));
  return readJson<ControlCenterJobListItem[]>(response);
}

export async function fetchJobById(id: string): Promise<ControlCenterJobDetail> {
  const response = await fetch(buildUrl(`/api/v1/control-center/jobs/${id}`));
  return readJson<ControlCenterJobDetail>(response);
}
