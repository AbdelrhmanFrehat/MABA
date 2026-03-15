import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorHandlerService } from './error-handler.service';
import { MessageService } from 'primeng/api';

/**
 * Global HTTP error interceptor: shows a toast for every failed HTTP response
 * using ErrorHandlerService, then rethrows so callers still receive the error.
 * Runs after auth interceptor (401 is handled there; we still show a toast for session expired).
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const errorHandler = inject(ErrorHandlerService);
    const messageService = inject(MessageService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            errorHandler.showError(messageService, error);
            return throwError(() => error);
        })
    );
};
