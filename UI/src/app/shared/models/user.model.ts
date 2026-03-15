export interface User {
    id?: number;
    username: string;
    fullName: string;
    password?: string;
    isActive: boolean;
    roles?: string[];
    roleIds?: number[];
}

export interface Role {
    id?: number;
    name: string;
    description?: string;
}

export interface UserListResponse {
    items: User[];
    totalCount: number;
}

export interface RoleListResponse {
    items: Role[];
    totalCount: number;
}
