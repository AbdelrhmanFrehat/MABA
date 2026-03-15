// Auth Models
export interface RegisterRequest {
    fullName: string;
    email: string;
    phone?: string;
    password: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface AuthResponse {
    token: string;
    refreshToken: string;
    expiresAt: string;
    user: User;
}

export interface User {
    id: string; // GUID
    fullName: string;
    email: string;
    phone?: string;
    roles: string[];
    isActive?: boolean;
    createdAt?: string;
    updatedAt?: string;
}


