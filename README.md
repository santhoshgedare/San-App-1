# IdentityHub

A modern, minimal reference implementation of **user management, authentication, and role-based
authorization** — rebuilt from scratch with a current, clean architecture. It is intentionally
scoped to the identity domain (login/logout, registration, users, roles) rather than replicating
a full ERP-style application.

## What's inside

- **Backend** — ASP.NET Core 10 Web API using Clean Architecture:
  - `IdentityHub.Domain` — framework-free entities and constants.
  - `IdentityHub.Application` — CQRS use cases (MediatR + FluentValidation), only depends on Domain.
  - `IdentityHub.Infrastructure` — EF Core 10 (SQL Server), ASP.NET Core Identity, JWT issuing.
  - `IdentityHub.Api` — thin controllers, Swagger, JWT bearer auth, global exception handling.
- **Frontend** — Angular 22 (standalone components, signals, lazy-loaded routes) in `client/`, styled with Angular Material (toolbar + sidenav shell, bottom sheet settings panel) and Bootstrap 5 utility classes/components for forms and tables, following the buyer-spa layout pattern (fixed toolbar, collapsible side navigation, quick-access settings bottom sheet) from the legacy ProcNext SCM app.

## Features

- Register / Login / Logout with JWT access tokens + rotating refresh tokens.
- Role-based authorization (`Admin`, `Manager`, `User`) enforced both in API controllers and
  Angular route guards.
- User management: list, activate/deactivate, delete, assign roles.
- Role management: list, create, delete (with assigned user counts).
- **Module &gt; Page &gt; Section access masters**: a simplified alternative to the legacy app's
  three-table `RoleAccess*` design — a single `RoleSectionAccess` join table grants a role
  visibility into specific Sections, with Pages/Modules organizing them hierarchically for the UI.
  Drives the Angular `canRender` structural directive (`*canRender="'section-key'"`, the
  equivalent of the legacy `*appCanRender`) and the dynamic settings bottom sheet, which renders
  only the modules/pages the signed-in user's role(s) grant. Admins implicitly see everything.
  Manage per-role section access from **Roles → Manage Access**.
- "My Profile" and "Dashboard" pages showing the current session's user.
- Seeded default roles and a bootstrap admin account (`admin@identityhub.local` / `Admin@12345`).

## Architecture decisions (what changed vs. the legacy AdminPanel)

- Replaced hand-rolled JWT claims/token code with a small, testable `TokenService` +
  `IRefreshTokenService` pair, backed by a `RefreshTokens` table (rotation + revocation).
- Application layer depends only on interfaces (`IIdentityService`, `ITokenService`,
  `ICurrentUserService`) — Infrastructure (EF Core / ASP.NET Identity) is fully swappable.
- CQRS via MediatR with a FluentValidation pipeline behavior — no manual `ModelState` plumbing.
- Angular rebuilt on standalone components + signals (no NgModules, no zone.js reliance for state),
  lazy-loaded feature routes, and a functional `HttpInterceptorFn` that transparently refreshes
  expired access tokens.
- Module/Page/Section masters simplified vs. the legacy app: one `RoleSectionAccess(RoleId, SectionId)`
  join table instead of three parallel `RoleAccessModule`/`RoleAccessPage`/`RoleAccessSection` tables —
  granting a Section is the only access primitive; Page/Module visibility is derived from it.

## Running locally

### API

```powershell
dotnet run --project src/IdentityHub.Api
```

The API applies EF Core migrations and seeds roles/admin user on startup. Update
`src/IdentityHub.Api/appsettings.json` `ConnectionStrings:DefaultConnection` and `Jwt:Secret`
for your environment (use a strong secret and a secrets manager in production).

#### Google and Facebook (Meta) sign-in

For local development, create OAuth apps with these authorized redirect URIs:

- Google: `https://localhost:7226/signin-google`
- Facebook (Meta): `https://localhost:7226/signin-facebook`

For local development, create the gitignored
`src/IdentityHub.Api/appsettings.Local.json` file and add provider app credentials under `Authentication`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "<Google client ID>",
      "ClientSecret": "<Google client secret>"
    },
    "Facebook": {
      "AppId": "<Meta app ID>",
      "AppSecret": "<Meta app secret>"
    }
  }
}
```

#### Production configuration

Copy `src/IdentityHub.Api/appsettings.json` to the deployment host as
`src/IdentityHub.Api/appsettings.Production.json` and set the production values there. That file is
gitignored so provider, SMTP, JWT, and database secrets are not committed. Do not put API secrets in
Angular environment files:

```json
{
  "AllowedOrigins": ["https://shop.example.com"],
  "Client": { "BaseUrl": "https://shop.example.com" },
  "Authentication": {
    "Google": {
      "ClientId": "<production Google client ID>",
      "ClientSecret": "<production Google client secret>"
    },
    "Facebook": {
      "AppId": "<production Meta app ID>",
      "AppSecret": "<production Meta app secret>"
    }
  },
  "Email": {
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "Username": "<SMTP username>",
      "Password": "<SMTP password>",
      "FromAddress": "accounts@example.com",
      "FromName": "SRIVIDIKA",
      "UseStartTls": true,
      "TimeoutSeconds": 15
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "<production SQL Server connection string>"
  },
  "Jwt": {
    "Secret": "<long random production signing secret>",
    "Issuer": "IdentityHub",
    "Audience": "IdentityHubClient",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  }
}
```

Set `ASPNETCORE_ENVIRONMENT=Production` in the hosting platform so ASP.NET Core loads this file.
Keep the production override on the server and restrict file access; both local and production
override files are gitignored. OAuth credentials are read from `Authentication:Google` and
`Authentication:Facebook` in appsettings.
Password recovery is limited to
five requests per source IP per hour by default; `PasswordRecovery:RateLimit` and
`PasswordRecovery:TokenLifespanMinutes` in `appsettings.json` control these values. SMTP transport
settings are bound from `Email:Smtp`, and the reset-link origin comes from `Client:BaseUrl`. The
committed `appsettings.json` includes safe local defaults with blank SMTP credentials. Forgot-password responses are generic to prevent account
enumeration; password changes send a confirmation email. The API returns `503` for recovery while
SMTP is not configured. Set `Client__BaseUrl` to the exact public client origin so reset links
return to the deployed reset-password page.

Replace `https://shop.example.com` with the exact browser origin serving the client (scheme and
host, plus port if nonstandard). `environment.prod.ts` uses `/api`, so the default setup expects
the client and API to share one public origin. If they are hosted separately, set `AllowedOrigins`
to the client origin and change `client/src/environments/environment.prod.ts` to the public API
base URL before building the client.

In the Google Cloud OAuth client and Meta Facebook Login settings, add these **public API** redirect
URIs, replacing the host with the deployed API host:

- Google: `https://api.example.com/signin-google`
- Facebook (Meta): `https://api.example.com/signin-facebook`

For a same-origin deployment, use the app host instead (for example,
`https://shop.example.com/signin-google`). If TLS terminates at a reverse proxy, configure the
hosting platform and ASP.NET Core forwarded-header handling so OAuth sees the original HTTPS
scheme and public host. After setting secrets and provider redirect URIs, restart/redeploy the API.
The login page enables each provider only when its credentials are present. Google accounts with
verified email can link to an existing account; unverified provider emails cannot silently link.

### Client

```powershell
cd client
npm install
npm start
```

Serves on `http://localhost:4200` and expects the API at `https://localhost:7226/api`
(configurable in `client/src/environments/environment.ts`).

## Tests

```powershell
dotnet test tests/IdentityHub.Application.Tests
```
