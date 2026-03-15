import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';

export interface ErrorInfo {
    message: string;
    code?: number;
    type: 'error' | 'warning' | 'info';
}

@Injectable({
    providedIn: 'root'
})
export class ErrorHandlerService {
    private translateService = inject(TranslateService);

    handleError(error: HttpErrorResponse | Error): ErrorInfo {
        if (error instanceof HttpErrorResponse) {
            return this.handleHttpError(error);
        }
        
        return {
            message: error.message || this.translateService.instant('messages.genericError'),
            type: 'error'
        };
    }

    private handleHttpError(error: HttpErrorResponse): ErrorInfo {
        let message = this.translateService.instant('messages.genericError');

        if (error.error instanceof ErrorEvent) {
            message = error.error.message;
        } else {
            switch (error.status) {
                case 0:
                    message = this.translateService.instant('messages.networkError');
                    break;
                case 400:
                    message = this.extractErrorMessage(error) || this.translateService.instant('messages.badRequest');
                    break;
                case 401:
                    message = this.translateService.instant('messages.sessionExpired');
                    break;
                case 403:
                    message = this.translateService.instant('messages.forbidden');
                    break;
                case 404:
                    message = this.translateService.instant('messages.notFound');
                    break;
                case 422:
                    message = this.extractValidationErrors(error) || this.translateService.instant('messages.validationError');
                    break;
                case 429:
                    message = this.extractErrorMessage(error) || this.translateService.instant('messages.tooManyRequests');
                    break;
                case 500:
                    message = this.translateService.instant('messages.serverError');
                    break;
                case 503:
                    message = this.translateService.instant('messages.serviceUnavailable');
                    break;
                default:
                    message = this.extractErrorMessage(error) || this.translateService.instant('messages.genericError');
                    break;
            }
        }

        return {
            message,
            code: error.status,
            type: error.status >= 500 ? 'error' : 'warning'
        };
    }

    private extractErrorMessage(error: HttpErrorResponse): string | null {
        if (typeof error.error === 'string') {
            return error.error;
        }
        if (error.error?.message) {
            return error.error.message;
        }
        if (error.error?.title) {
            return error.error.title;
        }
        return null;
    }

    private extractValidationErrors(error: HttpErrorResponse): string | null {
        if (error.error?.errors) {
            const errors = error.error.errors;
            if (typeof errors === 'object') {
                const validationMessages = Object.values(errors).flat() as string[];
                return validationMessages.join('. ');
            }
        }
        return null;
    }

    showError(messageService: any, error: HttpErrorResponse | Error): void {
        const errorInfo = this.handleError(error);
        messageService.add({
            severity: errorInfo.type,
            summary: this.translateService.instant('messages.error'),
            detail: errorInfo.message,
            life: 5000
        });
    }
}
