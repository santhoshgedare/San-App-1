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
