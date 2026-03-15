import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Machine,
    CreateMachineRequest,
    UpdateMachineRequest,
    MachinePart,
    CreateMachinePartRequest,
    ItemMachineLink,
    CreateItemMachineLinkRequest
} from '../models/machine.model';

@Injectable({
    providedIn: 'root'
})
export class MachinesApiService {
    private baseUrl = `${environment.apiUrl}/machines`;

    constructor(private http: HttpClient) {}

    getAllMachines(): Observable<Machine[]> {
        return this.http.get<Machine[]>(this.baseUrl);
    }

    getMachineById(id: string): Observable<Machine> {
        return this.http.get<Machine>(`${this.baseUrl}/${id}`);
    }

    createMachine(request: CreateMachineRequest): Observable<Machine> {
        return this.http.post<Machine>(this.baseUrl, request);
    }

    updateMachine(id: string, request: UpdateMachineRequest): Observable<Machine> {
        return this.http.put<Machine>(`${this.baseUrl}/${id}`, request);
    }

    deleteMachine(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    // Machine Parts
    getMachineParts(params?: {
        page?: number;
        pageSize?: number;
        machineId?: string;
        search?: string;
    }): Observable<{ items: MachinePart[]; totalCount: number; page: number; pageSize: number; totalPages: number }> {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                const value = params[key as keyof typeof params];
                if (value !== null && value !== undefined && value !== '') {
                    httpParams = httpParams.set(key, value.toString());
                }
            });
        }
        return this.http.get<{ items: MachinePart[]; totalCount: number; page: number; pageSize: number; totalPages: number }>(`${this.baseUrl}/parts`, { params: httpParams });
    }

    getMachinePartById(id: string): Observable<MachinePart> {
        return this.http.get<MachinePart>(`${this.baseUrl}/parts/${id}`);
    }

    createMachinePart(request: CreateMachinePartRequest): Observable<MachinePart> {
        return this.http.post<MachinePart>(`${this.baseUrl}/parts`, request);
    }

    updateMachinePart(id: string, request: Partial<CreateMachinePartRequest>): Observable<MachinePart> {
        return this.http.put<MachinePart>(`${this.baseUrl}/parts/${id}`, request);
    }

    deleteMachinePart(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/parts/${id}`);
    }

    // Item-Machine Links
    createItemMachineLink(request: CreateItemMachineLinkRequest): Observable<ItemMachineLink> {
        return this.http.post<ItemMachineLink>(`${this.baseUrl}/links`, request);
    }

    getItemMachineLinks(itemId?: string, machineId?: string): Observable<ItemMachineLink[]> {
        let params = new HttpParams();
        if (itemId) {
            params = params.set('itemId', itemId);
        }
        if (machineId) {
            params = params.set('machineId', machineId);
        }
        return this.http.get<ItemMachineLink[]>(`${this.baseUrl}/links`, { params });
    }

    deleteItemMachineLink(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/item-machine-links/${id}`);
    }
}


