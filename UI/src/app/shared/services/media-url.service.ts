import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class MediaUrlService {
  /**
   * Builds a full image URL from a relative or absolute path.
   * - If the path is already absolute (http/https), it is returned as-is.
   * - If the path is relative (e.g. /uploads/...), it is prefixed with imageBaseUrl.
   */
  getUrl(path: string | null | undefined): string {
    if (!path || !path.trim()) {
      return 'assets/img/defult.png';
    }

    const trimmed = path.trim();
    if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
      return trimmed;
    }

    const base = (environment as any).imageBaseUrl as string | undefined;
    if (!base) {
      return trimmed;
    }

    const normalizedBase = base.endsWith('/') ? base.slice(0, -1) : base;
    const normalizedPath = trimmed.startsWith('/') ? trimmed : '/' + trimmed;
    return normalizedBase + normalizedPath;
  }
}

