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

export enum ProjectWorkflowStatus {
    New = 'New',
    UnderReview = 'UnderReview',
    WaitingForInfo = 'WaitingForInfo',
    TechnicalReview = 'TechnicalReview',
    QuotationPreparation = 'QuotationPreparation',
    QuoteSent = 'QuoteSent',
    Approved = 'Approved',
    InExecution = 'InExecution',
    Completed = 'Completed',
    Rejected = 'Rejected'
}

export enum ProjectRequestPriority {
    Low = 'Low',
    Medium = 'Medium',
    High = 'High',
    Critical = 'Critical'
}

export enum ProjectRequestFeasibility {
    Feasible = 'Feasible',
    PartiallyFeasible = 'PartiallyFeasible',
    NotFeasible = 'NotFeasible'
}

export enum ProjectRequestComplexity {
    Simple = 'Simple',
    Moderate = 'Moderate',
    Complex = 'Complex'
}

export interface ProjectRequestActivity {
    id: string;
    actionType: string;
    description: string;
    createdBy?: string;
    createdAt: string;
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
    projectType?: ProjectRequestProjectType | string;
    mainDomain?: ProjectRequestMainDomain | string;
    requiredCapabilities: string[];
    category?: ProjectCategory;
    categoryName?: string;
    budgetRange?: string;
    timeline?: string;
    description?: string;
    projectDescription?: string;
    projectStage?: ProjectRequestStage | string;
    attachmentUrl?: string;
    attachmentFileName?: string;
    attachments: ProjectRequestAttachment[];
    status: ProjectRequestStatus;
    statusName: string;
    workflowStatus: string;
    adminNotes?: string;
    assignedToUserId?: string;
    assignedToName?: string;
    priority?: string;
    technicalFeasibility?: string;
    estimatedCost?: number;
    estimatedTimeline?: string;
    complexityLevel?: string;
    internalNotes?: string;
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

export enum ProjectRequestProjectType {
    CustomProject = 'CustomProject',
    BasedOnExistingProduct = 'BasedOnExistingProduct',
    UpgradeModification = 'UpgradeModification',
    ReverseEngineeringRepair = 'ReverseEngineeringRepair'
}

export enum ProjectRequestMainDomain {
    Robotics = 'Robotics',
    EmbeddedSystems = 'EmbeddedSystems',
    MedicalDevices = 'MedicalDevices',
    IoTSystems = 'IoTSystems',
    IndustrialAutomation = 'IndustrialAutomation',
    MechanicalSystem = 'MechanicalSystem',
    SoftwareSystem = 'SoftwareSystem',
    MixedSystem = 'MixedSystem'
}

export enum ProjectRequestStage {
    Idea = 'Idea',
    ConceptReady = 'ConceptReady',
    PrototypeExists = 'PrototypeExists',
    NeedsManufacturing = 'NeedsManufacturing',
    NeedsImprovement = 'NeedsImprovement'
}

export enum ProjectRequestCapability {
    CadDesign = 'CadDesign',
    Modeling3d = 'Modeling3d',
    Printing3d = 'Printing3d',
    CncMachining = 'CncMachining',
    LaserCutting = 'LaserCutting',
    LaserEngraving = 'LaserEngraving',
    PcbDesign = 'PcbDesign',
    EmbeddedProgramming = 'EmbeddedProgramming',
    ElectronicsIntegration = 'ElectronicsIntegration',
    MechanicalAssembly = 'MechanicalAssembly',
    Prototyping = 'Prototyping',
    TestingValidation = 'TestingValidation',
    SoftwareDevelopment = 'SoftwareDevelopment',
    UiControlPanelDevelopment = 'UiControlPanelDevelopment',
    ReverseEngineering = 'ReverseEngineering'
}

export interface ProjectRequestAttachment {
    url: string;
    fileName: string;
}

export interface CreateProjectRequestPayload {
    fullName: string;
    email: string;
    phone?: string;
    requestType: ProjectRequestType;
    projectId?: string;
    projectType?: ProjectRequestProjectType | string;
    mainDomain?: ProjectRequestMainDomain | string;
    requiredCapabilities?: string[];
    category?: ProjectCategory;
    budgetRange?: string;
    timeline?: string;
    description?: string;
    projectDescription?: string;
    projectStage?: ProjectRequestStage | string;
    attachmentUrl?: string;
    attachmentFileName?: string;
    attachments?: ProjectRequestAttachment[];
}

export interface UpdateProjectRequestPayload {
    status?: ProjectRequestStatus;
    workflowStatus?: string;
    adminNotes?: string;
    fullName?: string;
    email?: string;
    phone?: string;
    requestType?: ProjectRequestType;
    projectId?: string;
    projectType?: ProjectRequestProjectType | string;
    mainDomain?: ProjectRequestMainDomain | string;
    requiredCapabilities?: string[];
    category?: ProjectCategory;
    budgetRange?: string;
    timeline?: string;
    projectStage?: ProjectRequestStage | string;
    description?: string;
    projectDescription?: string;
    attachmentUrl?: string;
    attachmentFileName?: string;
    attachments?: ProjectRequestAttachment[];
    assignedToUserId?: string;
    assignedToName?: string;
    priority?: string;
    technicalFeasibility?: string;
    estimatedCost?: number;
    estimatedTimeline?: string;
    complexityLevel?: string;
    internalNotes?: string;
    updatedBy?: string;
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
