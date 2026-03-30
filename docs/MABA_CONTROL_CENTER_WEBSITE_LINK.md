# Linking Maba Control Center (Desktop) with the MABA Website

This document describes what the **Maba Control Center** WPF desktop app needs to connect to the same backend and user accounts as the MABA website (Angular + .NET API).

---

## 1. What “link with the website” means

- **Same API:** The desktop app calls the same MABA API (e.g. `https://api.mabasol.com` or `http://localhost:5000` in dev) that the website uses.
- **Same user:** The user can log in with the same account (email + password) and get a JWT; optional: “Link this app” from the website so the desktop is tied to their account.
- **Shared data (optional):** User profile, preferences, or device/module data can be synced or associated with the web account.

---

## 2. What the desktop app needs

### 2.1 Configuration

- **API base URL**  
  - Add a setting (e.g. in `AppSettings` and Settings UI): `ApiBaseUrl` (e.g. `https://api.mabasol.com` or `http://localhost:5000`).  
  - Persist it via `SettingsService` (e.g. in `settings.json` under `%LocalAppData%\MabaControlCenter\`).  
  - Use this URL for all HTTP calls to the API (no hardcoded host).

### 2.2 HTTP client and auth

- **HTTP client**  
  - Use `HttpClient` (or Refit) in a small **API client service** (e.g. `IMabaApiService` / `MabaApiService`).  
  - Base address = `ApiBaseUrl` (e.g. `https://api.mabasol.com`).  
  - All requests to the API go through this client.

- **Authentication**  
  - **Login:** Call `POST /api/v1/auth/login` with `{ "email": "...", "password": "..." }`.  
  - **Response:** API returns JWT in `token` (and optionally `refreshToken`, `expiresAt`).  
  - **Store:** Save the token (and refresh token if used) securely:  
    - e.g. in a file under `%LocalAppData%\MabaControlCenter\` (encrypted or protected), or  
    - Windows Credential Manager, or another secure store.  
  - **Do not store** the user’s password; only tokens.

- **Authenticated requests**  
  - For endpoints that require login, send:  
    - Header: `Authorization: Bearer <token>`.  
  - Example: `GET /api/v1/auth/me` returns the current user when the token is valid.

- **Token expiry**  
  - JWT has an expiry (e.g. 60 minutes). Options:  
    - Use **refresh token** if the API supports it (e.g. `POST /api/v1/auth/refresh-token`).  
    - Or prompt the user to log in again when the API returns 401.

### 2.3 Where to use the API in the desktop app

- **Login / account**  
  - Optional “Login” or “Link account” in the desktop app (e.g. in Settings or a dedicated screen):  
    - User enters email + password → call `POST /api/v1/auth/login` → store token and optionally user info.  
  - On startup, if a token exists, call `GET /api/v1/auth/me` to validate and load user (optional).

- **Updates / news (existing services)**  
  - **UpdateService:** If app updates are served from your backend, call e.g. `GET /api/v1/.../update-check` (you’d add this endpoint if needed) using the same `ApiBaseUrl`.  
  - **NewsService:** If news comes from the API, call the appropriate endpoint (e.g. from CMS or a dedicated news API) with the same base URL.  
  - Both can use the same `HttpClient` / API service; authenticated endpoints would send the Bearer token.

- **Future: device/module sync**  
  - If you later want “devices” or “modules” in the desktop to be associated with the web account, you’d add API endpoints (e.g. `GET /api/v1/users/me/devices`) and call them from the desktop with the stored token.

### 2.4 Security

- **HTTPS in production:** Use `https://` for `ApiBaseUrl` in production.  
- **No password storage:** Store only JWT (and refresh token if used).  
- **Secure token storage:** Prefer a protected/encrypted store (e.g. DPAPI on Windows or Credential Manager) rather than plain text in a file.  
- **Certificate validation:** Leave default `HttpClient` certificate validation on for production; only disable for dev if necessary and only in debug builds.

---

## 3. API endpoints relevant to the desktop app

(These already exist on the MABA API unless noted.)

| Purpose              | Method | Endpoint                      | Auth   | Notes                          |
|----------------------|--------|-------------------------------|--------|--------------------------------|
| Login                | POST   | `/api/v1/auth/login`          | No     | Body: `email`, `password`      |
| Refresh token        | POST   | `/api/v1/auth/refresh-token`  | No     | Body: `refreshToken`            |
| Current user         | GET    | `/api/v1/auth/me`             | Bearer | Returns user profile            |
| (Future) Update check| GET    | e.g. `/api/v1/app/update`     | Optional | You can add this if needed   |
| (Future) News        | GET    | From CMS or custom            | Optional | Use existing or new endpoint  |

---

## 4. Suggested implementation steps (desktop app)

1. **Add settings**  
   - `ApiBaseUrl` (string), optionally “Remember me” / “Linked account” (e.g. email) in `AppSettings` and Settings UI.  
   - Persist and load via `SettingsService`.

2. **Add API + auth services**  
   - `IMabaApiService`:  
     - `LoginAsync(email, password)` → call `POST /api/v1/auth/login`, return token (and user info if returned).  
     - `GetCurrentUserAsync()` → `GET /api/v1/auth/me` with Bearer token.  
     - Optionally: `RefreshTokenAsync()` if you use refresh tokens.  
   - `IAuthStorageService` (or similar): save/load token (and refresh token) securely; clear on logout.

3. **Optional: Login / “Link account” UI**  
   - In Settings or a small “Account” section:  
     - Fields: Email, Password; button: “Log in” / “Link with my account”.  
     - On success: save token, show “Logged in as …” and maybe a “Log out” button.  
   - On app startup: if token exists, call `GET /api/v1/auth/me`; if 401, clear token and treat as not linked.

4. **Use ApiBaseUrl everywhere**  
   - UpdateService and NewsService (and any future API calls) should use the same `ApiBaseUrl` and, if needed, the same `HttpClient` with Bearer token for protected endpoints.

5. **Handle 401**  
   - If any request returns 401, clear the stored token and optionally show “Session expired; please log in again” (or open the login/link UI).

---

## 5. Website side (optional “Link desktop” flow)

If you want users to **start on the website** and then “connect” the desktop app:

- **Option A – Same login:** User opens the desktop app and logs in with the same email/password; no change needed on the website.  
- **Option B – Device/link code:**  
  - On the website: “Connect desktop app” → backend generates a short-lived code (e.g. 6 digits) and associates it with the user.  
  - User enters that code in the desktop app; desktop calls e.g. `POST /api/v1/auth/link-device` with the code and receives a JWT.  
  - This requires adding a “link device” or “device code” endpoint and a small UI on the website; the desktop app would have a “Enter code from website” screen.

For a first version, **Option A** (desktop login with same email/password and existing `POST /api/v1/auth/login`) is enough to “link” the program with the website in the sense of shared identity and API.

---

## 6. Summary checklist (desktop app)

- [ ] Add `ApiBaseUrl` to settings and UI; persist and load on startup.  
- [ ] Add HTTP client (e.g. `MabaApiService`) that uses `ApiBaseUrl`.  
- [ ] Implement login: `POST /api/v1/auth/login` → store token securely.  
- [ ] Implement token storage (and clear on logout / 401).  
- [ ] For protected calls: send `Authorization: Bearer <token>`.  
- [ ] Optional: Login / “Link account” UI in Settings or dedicated screen.  
- [ ] Optional: On startup, validate token with `GET /api/v1/auth/me`.  
- [ ] Wire UpdateService / NewsService to use the same API base (and auth if needed).  
- [ ] Use HTTPS and secure token storage in production.

Once these are in place, the desktop app is linked to the same backend and user accounts as the website; you can later add more endpoints (e.g. device list, sync) as needed.
