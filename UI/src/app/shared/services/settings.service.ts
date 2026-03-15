import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Settings } from '../models';

@Injectable({
    providedIn: 'root'
})
export class SettingsService {
    private apiUrl = `${environment.apiUrl}/Settings`;

    constructor(private http: HttpClient) {}

    get(): Observable<Settings> {
        return this.http.get<Settings>(this.apiUrl);
    }

    update(settings: Settings): Observable<void> {
        return this.http.put<void>(this.apiUrl, settings);
    }
}

