/**
 * Sanitizes a returnUrl from query params so it is safe to use for post-login redirect.
 * - Only allows relative app paths (single leading /).
 * - Rejects external URLs, /notfound, and auth pages to avoid loops.
 */
export function sanitizeReturnUrl(returnUrl: string | undefined | null): string {
    if (returnUrl == null || typeof returnUrl !== 'string') {
        return '/';
    }
    const trimmed = returnUrl.trim();
    if (!trimmed) {
        return '/';
    }
    // Reject external or protocol-relative URLs
    if (trimmed.startsWith('//') || /^https?:\/\//i.test(trimmed)) {
        return '/';
    }
    // Ensure absolute path
    const path = trimmed.startsWith('/') ? trimmed : '/' + trimmed;
    // Avoid redirecting to 404 or auth pages
    if (path === '/notfound' || path.startsWith('/auth/login') || path.startsWith('/auth/register')) {
        return '/';
    }
    return path;
}

/**
 * Normalizes a URL for use as returnUrl (e.g. from router state).
 * Ensures the value is an absolute path so the login page receives a valid route.
 */
export function normalizeReturnUrl(url: string | undefined | null): string {
    if (url == null || typeof url !== 'string') {
        return '/';
    }
    const trimmed = url.trim();
    if (!trimmed) {
        return '/';
    }
    return trimmed.startsWith('/') ? trimmed : '/' + trimmed;
}
