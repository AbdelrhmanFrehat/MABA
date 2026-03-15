export interface Project {
    id: string;
    titleEn: string;
    titleAr?: string;
    slug: string;
    summaryEn?: string;
    summaryAr?: string;
    descriptionEn?: string;
    descriptionAr?: string;
    coverImageUrl?: string;
    category: ProjectCategory;
    categoryName: string;
    status: ProjectStatus;
    statusName: string;
    year: number;
    techStack: string[];
    highlights: string[];
    gallery: string[];
    isFeatured: boolean;
    isActive: boolean;
    sortOrder: number;
    createdAt: string;
    updatedAt?: string;
}

export interface ProjectListItem {
    id: string;
    titleEn: string;
    titleAr?: string;
    slug: string;
    summaryEn?: string;
    summaryAr?: string;
    coverImageUrl?: string;
    category: ProjectCategory;
    categoryName: string;
    status: ProjectStatus;
    statusName: string;
    year: number;
    techStack: string[];
    isFeatured: boolean;
}

export interface ProjectsListResponse {
    items: ProjectListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
}

export enum ProjectCategory {
    Electronics = 0,
    Mechanical = 1,
    Software = 2,
    Automation = 3,
    Custom = 4,
    Robotics = 5,
    CNC = 6,
    Monitoring = 7,
    Embedded = 8,
    RnD = 9
}

export enum ProjectStatus {
    Draft = 0,
    InProgress = 1,
    Completed = 2,
    Published = 3,
    Archived = 4,
    Delivered = 5,
    Prototype = 6,
    Concept = 7
}

export interface ProjectRequest {
    id: string;
    referenceNumber: string;
    fullName: string;
    email: string;
    phone?: string;
    requestType: ProjectRequestType;
    requestTypeName: string;
    projectId?: string;
    projectTitle?: string;
    category?: ProjectCategory;
    categoryName?: string;
    budgetRange?: string;
    timeline?: string;
    description?: string;
    attachmentUrl?: string;
    attachmentFileName?: string;
    status: ProjectRequestStatus;
    statusName: string;
    adminNotes?: string;
    createdAt: string;
    updatedAt?: string;
}

export enum ProjectRequestType {
    SimilarToExisting = 0,
    Custom = 1
}

export enum ProjectRequestStatus {
    New = 0,
    InReview = 1,
    Quoted = 2,
    InProgress = 3,
    Closed = 4
}

export interface CreateProjectRequestPayload {
    fullName: string;
    email: string;
    phone?: string;
    requestType: ProjectRequestType;
    projectId?: string;
    category?: ProjectCategory;
    budgetRange?: string;
    timeline?: string;
    description?: string;
}

export interface CreateProjectPayload {
    titleEn: string;
    titleAr?: string;
    slug?: string;
    summaryEn?: string;
    summaryAr?: string;
    descriptionEn?: string;
    descriptionAr?: string;
    coverImageUrl?: string;
    category: ProjectCategory;
    status?: ProjectStatus;
    year: number;
    techStack?: string[];
    highlights?: string[];
    gallery?: string[];
    isFeatured?: boolean;
    isActive?: boolean;
    sortOrder?: number;
}

export interface UpdateProjectPayload extends CreateProjectPayload {
    id: string;
}
