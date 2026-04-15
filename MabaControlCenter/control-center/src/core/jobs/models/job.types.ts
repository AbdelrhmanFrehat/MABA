export type JobSourceType =
  | "CNC_REQUEST"
  | "PRINT_REQUEST"
  | "LASER_REQUEST"
  | "ORDER";

export type JobMachineType =
  | "CNC"
  | "PRINTER_3D"
  | "LASER"
  | "DEXTER"
  | "SCARA";

export type JobStatus =
  | "Pending"
  | "Ready"
  | "InProgress"
  | "Completed"
  | "Failed"
  | "Cancelled";

export type JobAttachment = {
  fileName: string;
  filePath?: string | null;
  fileUrl?: string | null;
  kind?: string | null;
};

export type ControlCenterJobListItem = {
  id: string;
  jobReference: string;
  sourceType: string;
  sourceId?: string | null;
  sourceReference?: string | null;
  title: string;
  description?: string | null;
  customerName?: string | null;
  machineType?: string | null;
  moduleId?: string | null;
  status: string;
  priority?: string | null;
  assignedDeviceId?: string | null;
  createdAt: string;
  updatedAt?: string | null;
};

export type ControlCenterJobDetail = ControlCenterJobListItem & {
  startedAt?: string | null;
  completedAt?: string | null;
  resultSummary?: string | null;
  attachments: JobAttachment[];
  payloadJson?: string | null;
};
