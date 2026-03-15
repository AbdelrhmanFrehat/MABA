// Role & Permission Models
export interface Role {
    id: string; // GUID
    name: string;
    description?: string;
    permissions: Permission[];
    createdAt: string;
    updatedAt?: string;
}

export interface Permission {
    id: string; // GUID
    key: string;
    name: string;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateRoleRequest {
    name: string;
    description?: string;
    permissionIds?: string[];
}

export interface UpdateRoleRequest {
    name: string;
    description?: string;
    permissionIds?: string[];
}


