export interface DownloadableFileDto {
    id: string;
    entityType: string;
    entityId: string;
    title: string;
    description?: string;
    category: string;
    fileName: string;
    contentType: string;
    fileSizeBytes: number;
    sortOrder: number;
    isActive: boolean;
    isFeatured: boolean;
    downloadCount: number;
    createdAt: string;
}

export interface UpdateDownloadableFileRequest {
    title?: string;
    description?: string;
    category?: string;
    sortOrder?: number;
    isActive?: boolean;
    isFeatured?: boolean;
}

export const DOWNLOAD_CATEGORIES = [
    { value: 'Datasheet',         labelKey: 'downloads.categories.datasheet' },
    { value: 'UserManual',        labelKey: 'downloads.categories.userManual' },
    { value: 'TechnicalDocument', labelKey: 'downloads.categories.technicalDocument' },
    { value: 'Brochure',          labelKey: 'downloads.categories.brochure' },
    { value: 'Certificate',       labelKey: 'downloads.categories.certificate' },
    { value: 'InstallationGuide', labelKey: 'downloads.categories.installationGuide' },
    { value: 'Firmware',          labelKey: 'downloads.categories.firmware' },
    { value: 'Download',          labelKey: 'downloads.categories.download' },
    { value: 'Other',             labelKey: 'downloads.categories.other' },
] as const;

export type DownloadCategoryValue = (typeof DOWNLOAD_CATEGORIES)[number]['value'];

/** Returns the category icon class for display. */
export function categoryIcon(category: string): string {
    const map: Record<string, string> = {
        Datasheet:          'pi pi-file-pdf',
        UserManual:         'pi pi-book',
        TechnicalDocument:  'pi pi-file',
        Brochure:           'pi pi-images',
        Certificate:        'pi pi-verified',
        InstallationGuide:  'pi pi-wrench',
        Firmware:           'pi pi-microchip',
        Download:           'pi pi-download',
        Other:              'pi pi-paperclip',
    };
    return map[category] ?? 'pi pi-paperclip';
}

/** Returns a human-readable file size string. */
export function formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
