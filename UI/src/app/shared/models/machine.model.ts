// Machine Models
export interface Machine {
    id: string; // GUID
    nameEn: string;
    nameAr: string;
    manufacturer?: string;
    model?: string;
    yearFrom?: number;
    yearTo?: number;
    parts: MachinePart[];
    createdAt: string;
    updatedAt?: string;
}

export interface CreateMachineRequest {
    nameEn: string;
    nameAr: string;
    manufacturer?: string;
    model?: string;
    yearFrom?: number;
    yearTo?: number;
}

export interface UpdateMachineRequest {
    nameEn: string;
    nameAr: string;
    manufacturer?: string;
    model?: string;
    yearFrom?: number;
    yearTo?: number;
}

export interface MachinePart {
    id: string; // GUID
    machineId: string;
    partNameEn: string;
    partNameAr: string;
    partCode?: string;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateMachinePartRequest {
    machineId: string;
    partNameEn: string;
    partNameAr: string;
    partCode?: string;
}

export interface ItemMachineLink {
    id: string; // GUID
    itemId: string;
    machineId: string;
    machinePartId?: string;
    machineNameEn?: string;
    partNameEn?: string;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateItemMachineLinkRequest {
    itemId: string;
    machineId: string;
    machinePartId?: string;
}


