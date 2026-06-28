# MultiTenantSaaS

Multi-tenant SaaS backend and workspace UI with tenant-scoped data, JWT auth, and permission-based authorization.

## Services

- **Auth** — company signup, login, email verification, forgot/reset password, refresh token rotation, logout, reuse detection
- **Tenancy** — tenant resolution via `X-Tenant-Slug`, membership checks, EF Core global query filters
- **Authorization** — roles, permissions, policy-based API access
- **Users** — list, invite, accept invitation, assign role, disable
- **Workspace** — projects and tasks with tenant isolation
- **Tenant admin** — settings, roles/permissions, audit logs
- **Platform** — cross-tenant directory (create, update, enable/disable)
- **Security** — rate limiting (Redis/in-memory), identity lockout, audit logging

## Frontend

React app with public auth pages, user workspace (projects/tasks/profile), tenant admin console, and platform admin UI.

## Stack

| Area | Technologies |
|------|----------------|
| API | ASP.NET Core 8, EF Core, ASP.NET Identity |
| Data | PostgreSQL, Redis |
| Auth | JWT access tokens, refresh token store |
| UI | React 19, Vite, Tailwind CSS, React Router, Axios |

## Infrastructure

Postgres and Redis for local dev — [docker/README.md](docker/README.md).
