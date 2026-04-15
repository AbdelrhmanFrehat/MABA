import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useDeviceStore } from "../core/devices/store/device.store";
import type { DeviceType } from "../core/devices/models/device.types";
import {
  fetchJobs,
} from "../core/jobs/services/jobs.api";
import type {
  ControlCenterJobListItem,
  JobMachineType,
  JobStatus,
} from "../core/jobs/models/job.types";

const machineTypes: Array<{ label: string; value: JobMachineType }> = [
  { label: "CNC", value: "CNC" },
  { label: "3D Printer", value: "PRINTER_3D" },
  { label: "Laser", value: "LASER" },
  { label: "Dexter", value: "DEXTER" },
  { label: "SCARA", value: "SCARA" },
];

const statuses: JobStatus[] = [
  "Pending",
  "Ready",
  "InProgress",
  "Completed",
  "Failed",
  "Cancelled",
];

function machineLabel(machineType?: string | null): string {
  switch (machineType) {
    case "PRINTER_3D":
      return "3D Printer";
    default:
      return machineType ?? "Unknown";
  }
}

function sourceLabel(sourceType: string): string {
  switch (sourceType) {
    case "CNC_REQUEST":
      return "CNC Request";
    case "PRINT_REQUEST":
      return "3D Print Request";
    case "LASER_REQUEST":
      return "Laser Request";
    case "ORDER":
      return "Order";
    default:
      return sourceType;
  }
}

function mapMachineTypeToDevice(machineType?: string | null): DeviceType | null {
  switch (machineType) {
    case "CNC":
      return "CNC";
    case "DEXTER":
      return "DEXTER";
    case "SCARA":
      return "SCARA";
    default:
      return null;
  }
}

function compatibilityText(
  job: ControlCenterJobListItem,
  activeDeviceType?: DeviceType | null
): string {
  const requiredType = mapMachineTypeToDevice(job.machineType);
  if (!requiredType) {
    return "No simulated device match in app";
  }

  if (!activeDeviceType) {
    return `Compatible with ${requiredType}`;
  }

  return activeDeviceType === requiredType
    ? `Ready on active ${requiredType} device`
    : `Needs ${requiredType} device`;
}

function statusClass(status: string): string {
  return `status-badge status-${status.toLowerCase()}`;
}

export const JobsPage: React.FC = () => {
  const activeDevice = useDeviceStore((s) => s.getActiveDevice());
  const [jobs, setJobs] = useState<ControlCenterJobListItem[]>([]);
  const [statusFilter, setStatusFilter] = useState("");
  const [machineFilter, setMachineFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadJobs = async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await fetchJobs({
        status: statusFilter || undefined,
        machineType: machineFilter || undefined,
      });
      setJobs(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load jobs.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadJobs();
  }, [statusFilter, machineFilter]);

  const summary = useMemo(
    () => ({
      total: jobs.length,
      ready: jobs.filter((job) => job.status === "Ready").length,
      inProgress: jobs.filter((job) => job.status === "InProgress").length,
      completed: jobs.filter((job) => job.status === "Completed").length,
    }),
    [jobs]
  );

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ marginTop: 0, marginBottom: "0.35rem" }}>Jobs</h1>
          <p className="page-subtitle">
            Production jobs prepared from website service requests for operator visibility.
          </p>
        </div>
        <button type="button" className="secondary-button" onClick={() => void loadJobs()}>
          Refresh
        </button>
      </div>

      <div className="stats-grid">
        <div className="metric-card">
          <div className="metric-label">Total jobs</div>
          <div className="metric-value">{summary.total}</div>
        </div>
        <div className="metric-card">
          <div className="metric-label">Ready</div>
          <div className="metric-value">{summary.ready}</div>
        </div>
        <div className="metric-card">
          <div className="metric-label">In progress</div>
          <div className="metric-value">{summary.inProgress}</div>
        </div>
        <div className="metric-card">
          <div className="metric-label">Completed</div>
          <div className="metric-value">{summary.completed}</div>
        </div>
      </div>

      <div className="panel">
        <div className="filters-grid">
          <label className="field">
            <span>Status</span>
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
              <option value="">All statuses</option>
              {statuses.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </label>

          <label className="field">
            <span>Machine type</span>
            <select value={machineFilter} onChange={(e) => setMachineFilter(e.target.value)}>
              <option value="">All machine types</option>
              {machineTypes.map((machineType) => (
                <option key={machineType.value} value={machineType.value}>
                  {machineType.label}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <div className="panel">
        {loading ? <p>Loading jobs…</p> : null}
        {error ? <p className="error-text">{error}</p> : null}

        {!loading && !error ? (
          <table className="data-table">
            <thead>
              <tr>
                <th>Reference</th>
                <th>Title</th>
                <th>Source</th>
                <th>Machine</th>
                <th>Status</th>
                <th>Created</th>
                <th>Assigned</th>
                <th>Compatibility</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {jobs.length === 0 ? (
                <tr>
                  <td colSpan={9} className="empty-state-cell">
                    No jobs available yet.
                  </td>
                </tr>
              ) : (
                jobs.map((job) => (
                  <tr key={job.id}>
                    <td>{job.jobReference}</td>
                    <td>
                      <div>{job.title}</div>
                      {job.customerName ? (
                        <div className="muted-text">{job.customerName}</div>
                      ) : null}
                    </td>
                    <td>
                      <div>{sourceLabel(job.sourceType)}</div>
                      {job.sourceReference ? (
                        <div className="muted-text">{job.sourceReference}</div>
                      ) : null}
                    </td>
                    <td>{machineLabel(job.machineType)}</td>
                    <td>
                      <span className={statusClass(job.status)}>{job.status}</span>
                    </td>
                    <td>{new Date(job.createdAt).toLocaleString()}</td>
                    <td>{job.assignedDeviceId ?? "Unassigned"}</td>
                    <td>{compatibilityText(job, activeDevice?.type ?? null)}</td>
                    <td>
                      <Link className="inline-link" to={`/jobs/${job.id}`}>
                        Open
                      </Link>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        ) : null}
      </div>
    </div>
  );
};
