export interface SoftwareProduct {
    id: string;
    slug: string;
    nameEn: string;
    nameAr: string;
    summaryEn?: string;
    summaryAr?: string;
    descriptionEn?: string;
    descriptionAr?: string;
    category?: string;
    iconKey?: string;
    licenseEn?: string;
    licenseAr?: string;
    isActive: boolean;
    sortOrder: number;
    releasesCount?: number;
    latestVersion?: string;
    latestReleaseDate?: string;
    latestReleaseStatus?: string;
    downloadCount?: number;
    releases?: SoftwareRelease[];
}

export interface SoftwareRelease {
    id: string;
    version: string;
    releaseDate: string;
    status: string;
    changelogEn?: string;
    changelogAr?: string;
    requirementsEn?: string;
    requirementsAr?: string;
    isActive: boolean;
    files: SoftwareFile[];
}

export interface SoftwareFile {
    id: string;
    os: string;
    arch: string;
    fileType: string;
    fileName: string;
    fileSizeBytes: number;
    sha256?: string;
    downloadCount: number;
    isActive: boolean;
}

export interface CreateSoftwareProductRequest {
    slug: string;
    nameEn: string;
    nameAr: string;
    summaryEn?: string;
    summaryAr?: string;
    descriptionEn?: string;
    descriptionAr?: string;
    category?: string;
    iconKey?: string;
    licenseEn?: string;
    licenseAr?: string;
    isActive: boolean;
    sortOrder: number;
}

export interface CreateSoftwareReleaseRequest {
    version: string;
    releaseDate?: string;
    status: string;
    changelogEn?: string;
    changelogAr?: string;
    requirementsEn?: string;
    requirementsAr?: string;
    isActive: boolean;
}
