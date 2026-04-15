import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useDeviceStore } from "../core/devices/store/device.store";
import type { DeviceType } from "../core/devices/models/device.types";
import { fetchJobById } from "../core/jobs/services/jobs.api";
import type { ControlCenterJobDetail } from "../core/jobs/models/job.types";

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

function machineLabel(machineType?: string | null): string {
  switch (machineType) {
    case "PRINTER_3D":
      return "3D Printer";
    default:
      return machineType ?? "Unknown";
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

function parsePayload(payloadJson?: string | null): Array<[string, string]> {
  if (!payloadJson) {
    return [];
  }

  try {
    const parsed = JSON.parse(payloadJson) as Record<string, unknown>;
    return Object.entries(parsed)
      .filter(([, value]) => value !== null && value !== undefined && value !== "")
      .map(([key, value]) => [key, typeof value === "object" ? JSON.stringify(value) : String(value)]);
  } catch {
    return [["payload", payloadJson]];
  }
}

export const JobDetailsPage: React.FC = () => {
  const { jobId } = useParams();
  const activeDevice = useDeviceStore((s) => s.getActiveDevice());
  const [job, setJob] = useState<ControlCenterJobDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!jobId) {
      setError("Job id is missing.");
      setLoading(false);
      return;
    }

    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const result = await fetchJobById(jobId);
        setJob(result);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load job.");
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [jobId]);

  const compatibility = useMemo(() => {
    if (!job) {
      return "Unknown";
    }

    const requiredType = mapMachineTypeToDevice(job.machineType);
    if (!requiredType) {
      return "This job is ready for operator review, but no matching simulated device exists in the app yet.";
    }

    if (!activeDevice) {
      return `Requires a ${requiredType} device.`;
    }

    return activeDevice.type === requiredType
      ? `Active device ${activeDevice.name} is compatible with this job.`
      : `Active device ${activeDevice.name} is not compatible. Requires ${requiredType}.`;
  }, [activeDevice, job]);

  const payloadEntries = useMemo(() => parsePayload(job?.payloadJson), [job?.payloadJson]);

  if (loading) {
    return <p>Loading job…</p>;
  }

  if (error) {
    return (
      <div>
        <p className="error-text">{error}</p>
        <Link className="inline-link" to="/jobs">
          Back to jobs
        </Link>
      </div>
    );
  }

  if (!job) {
    return null;
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <Link className="inline-link" to="/jobs">
            ← Back to jobs
          </Link>
          <h1 style={{ marginTop: "0.5rem", marginBottom: "0.35rem" }}>{job.jobReference}</h1>
          <p className="page-subtitle">{job.title}</p>
        </div>
        <span className={`status-badge status-${job.status.toLowerCase()}`}>{job.status}</span>
      </div>

      <div className="details-grid">
        <div className="panel">
          <h2 className="section-title">Overview</h2>
          <dl className="details-list">
            <div>
              <dt>Source</dt>
              <dd>{sourceLabel(job.sourceType)}</dd>
            </div>
            <div>
              <dt>Source reference</dt>
              <dd>{job.sourceReference ?? job.sourceId ?? "N/A"}</dd>
            </div>
            <div>
              <dt>Machine type</dt>
              <dd>{machineLabel(job.machineType)}</dd>
            </div>
            <div>
              <dt>Customer</dt>
              <dd>{job.customerName ?? "N/A"}</dd>
            </div>
            <div>
              <dt>Created</dt>
              <dd>{new Date(job.createdAt).toLocaleString()}</dd>
            </div>
            <div>
              <dt>Assigned device</dt>
              <dd>{job.assignedDeviceId ?? "Unassigned"}</dd>
            </div>
          </dl>

          {job.description ? (
            <>
              <h3 className="subsection-title">Description</h3>
              <p className="prewrap-text">{job.description}</p>
            </>
          ) : null}
        </div>

        <div className="panel">
          <h2 className="section-title">Operator readiness</h2>
          <p>{compatibility}</p>

          {job.resultSummary ? (
            <>
              <h3 className="subsection-title">Result summary</h3>
              <p className="prewrap-text">{job.resultSummary}</p>
            </>
          ) : null}

          {job.startedAt || job.completedAt ? (
            <dl className="details-list">
              {job.startedAt ? (
                <div>
                  <dt>Started</dt>
                  <dd>{new Date(job.startedAt).toLocaleString()}</dd>
                </div>
              ) : null}
              {job.completedAt ? (
                <div>
                  <dt>Completed</dt>
                  <dd>{new Date(job.completedAt).toLocaleString()}</dd>
                </div>
              ) : null}
            </dl>
          ) : null}
        </div>
      </div>

      <div className="details-grid">
        <div className="panel">
          <h2 className="section-title">Files</h2>
          {job.attachments.length === 0 ? (
            <p className="muted-text">No files attached to this job snapshot.</p>
          ) : (
            <ul className="attachment-list">
              {job.attachments.map((attachment, index) => (
                <li key={`${attachment.fileName}-${index}`}>
                  <div>{attachment.fileName}</div>
                  <div className="muted-text">
                    {attachment.kind ?? "file"}
                    {attachment.fileUrl ? ` · ${attachment.fileUrl}` : ""}
                    {!attachment.fileUrl && attachment.filePath ? ` · ${attachment.filePath}` : ""}
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="panel">
          <h2 className="section-title">Job payload</h2>
          {payloadEntries.length === 0 ? (
            <p className="muted-text">No extra payload data was captured for this job yet.</p>
          ) : (
            <dl className="details-list">
              {payloadEntries.map(([key, value]) => (
                <div key={key}>
                  <dt>{key}</dt>
                  <dd className="prewrap-text">{value}</dd>
                </div>
              ))}
            </dl>
          )}
        </div>
      </div>
    </div>
  );
};
