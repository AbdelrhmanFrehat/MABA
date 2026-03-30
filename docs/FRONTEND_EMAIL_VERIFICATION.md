# Frontend: Email Verification Implementation Guide

The API now supports email verification for registration. Implement the following in the Angular app (when `UI/src` or the app source is available).

## 1. Verify-email page and route

- **Route:** Add `/auth/verify-email` (e.g. in the same auth routing module as login/register).
- **Component:** e.g. `VerifyEmailComponent`.
  - Read `token` from query params: `this.route.snapshot.queryParams['token']` or `this.route.queryParams.pipe(take(1))`.
  - If `token` is missing: show a message like "Missing or invalid link" and a link to login or to the resend flow.
  - If `token` is present:
    - Call `POST /api/v1/auth/verify-email` with body `{ "token": "<token>" }`.
    - On **success (200):** show a success view: "Email verified. You can now sign in." with a link to `/auth/login`.
    - On **failure (400):** show an error view: "Link invalid or expired." and optionally a "Resend verification email" action (e.g. link to login with a query like `?resend=1` or a small form to enter email and call resend).
- Use the existing HTTP client and error handling (e.g. `ErrorHandlerService` / `MessageService`) so errors are shown consistently.

## 2. API service methods

Add to your auth API service (or equivalent):

```ts
// Verify email with token from link
verifyEmail(token: string): Observable<{ success: boolean; message: string }> {
  return this.http.post<{ success: boolean; message: string }>(
    `${this.apiUrl}/auth/verify-email`,
    { token }
  );
}

// Resend verification email
resendVerification(email: string): Observable<{ success: boolean; message: string }> {
  return this.http.post<{ success: boolean; message: string }>(
    `${this.apiUrl}/auth/resend-verification`,
    { email }
  );
}
```

## 3. Register flow

- After calling `POST /api/v1/auth/register`, the API returns **201** with body `{ "message": "Registration successful. Please check your email to verify your account." }` (no token).
- Show that message in the UI and do **not** log the user in.
- Optionally show a "Resend verification email" action that asks for email and calls `POST /api/v1/auth/resend-verification`.

## 4. Login flow

- When calling `POST /api/v1/auth/login`, the API may return **403 Forbidden** with body e.g. `{ "error": "Email not verified", "message": "Please verify your email before signing in." }`.
- When you receive 403 with this message:
  - Show the message in the UI.
  - Optionally show a "Resend verification email" button that either:
    - Opens a small form/dialog to enter email and call resend, or
    - Navigates to a dedicated resend page or to login with `?resend=1` and a field for email.

## 5. Resend verification

- **Endpoint:** `POST /api/v1/auth/resend-verification` with body `{ "email": "<email>" }`.
- On success (200), show the response `message` (e.g. "A new verification link has been sent to your email." or "Email is already verified. You can sign in.").
- Rate limiting applies to auth endpoints; show a friendly message if the user hits the limit.

## 6. i18n keys (optional)

Add translation keys for:

- `auth.verifyEmailSuccess`, `auth.verifyEmailInvalidOrExpired`, `auth.verifyEmailMissingLink`
- `auth.pleaseVerifyEmailBeforeSignIn` (for 403 on login)
- `auth.resendVerification`, `auth.resendVerificationSuccess`, `auth.registrationCheckEmail`

Use these in the verify-email page and in login/register messages.

## 7. Routing example

In your auth routing module (or app routes), add:

```ts
{ path: 'verify-email', component: VerifyEmailComponent }
```

under the `auth` path so the full path is `/auth/verify-email`. The verification link sent by the API is `{FrontendBaseUrl}/verify-email.html?token=...` (static page) or you can implement an Angular route and point the API to `{FrontendBaseUrl}/auth/verify-email?token=...`.

## 8. Static verify-email page (no Angular route needed)

A standalone page is included so the email link works without adding an Angular route:

- **File:** `UI/dist/maba-ng/browser/verify-email.html`
- **URL:** The API sends `{FrontendBaseUrl}/verify-email.html?token=...` in the email.
- **Behaviour:** The page reads `token` from the query string, calls `POST /api/v1/auth/verify-email` with `{ "token": "..." }`, then shows success or error and a “Go to Sign in” link.
- **API URL:** On port 4200 it uses `http://localhost:5000/api/v1`; otherwise it uses the same origin. Override with `window.MABA_API_BASE = 'https://your-api'` if needed.

If you rebuild the Angular app, copy `verify-email.html` into your build output (e.g. into `assets` and configure so it is served at the app root), or add an Angular route as in §1–7.

## 9. Verification link and local / mobile testing

The link in the email is built from **`App:FrontendBaseUrl`** (API config). If this is `http://localhost:4200`, the link will only work when opened on the same machine. To test on a phone or another device:

- **Option A – Same network:** Set `App:FrontendBaseUrl` to your dev machine’s LAN URL, e.g. `http://192.168.1.100:4200`. Run the Angular app so it listens on all interfaces (e.g. `ng serve --host 0.0.0.0`). Restart the API after changing the config.
- **Option B – Tunnel (e.g. ngrok):** Run a tunnel to port 4200 and set `App:FrontendBaseUrl` to the tunnel URL (e.g. `https://abc123.ngrok.io`). Restart the API.

Override in `appsettings.Development.json` or User Secrets so you don’t commit the URL. Example for Development:

```json
"App": {
  "FrontendBaseUrl": "http://192.168.1.100:4200"
}
```
