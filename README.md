# MultiTenantSaaS

Multi-tenant authentication and authorization platform built with ASP.NET Core 8, PostgreSQL, Redis, and React (Vite + Tailwind).

## Stack

| Layer | Technology |
|-------|------------|
| API | ASP.NET Core 8, Swagger |
| Application | Clean architecture services |
| Data | PostgreSQL 16, EF Core, ASP.NET Identity |
| Cache | Redis 7 |
| Frontend | React + Vite + Tailwind (Faz 6) |

## Repository layout

```
MultiTenantSaaS/
├── src/
│   ├── MultiTenantSaaS.Api/
│   ├── MultiTenantSaaS.Application/
│   ├── MultiTenantSaaS.Domain/
│   ├── MultiTenantSaaS.Infrastructure/
│   └── MultiTenantSaaS.Shared/
├── tests/
│   └── MultiTenantSaaS.IntegrationTests/
├── docker/
│   └── docker-compose.yml
└── frontend/          # added in Faz 6
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Node.js 20+ (frontend, later phases)

## Quick start

### 1. Start infrastructure

```bash
cd docker
docker compose up -d
```

Services:

| Service | Port | Credentials |
|---------|------|-------------|
| PostgreSQL | 5432 | `postgres` / `postgres`, DB `multitenant_saas` |
| Redis | 6379 | no password (dev) |

### 2. Run the API

```bash
dotnet restore
dotnet build
dotnet run --project src/MultiTenantSaaS.Api
```

- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: http://localhost:5000/swagger
- Health: http://localhost:5000/health

### 3. Run tests

```bash
dotnet test
```

## Configuration

Connection strings live in `src/MultiTenantSaaS.Api/appsettings.Development.json` and match the Docker Compose defaults.

## Roadmap (high level)

- [x] **#1** Solution scaffold + Docker (PostgreSQL, Redis)
- [ ] **#2** Domain entities (Tenant, membership)
- [ ] **#3** Company signup API
- [ ] **#4** Initial EF migration
- [ ] Auth: JWT, refresh rotation, logout
- [ ] Multi-tenancy middleware + global filters
- [ ] RBAC, invitations, audit, rate limiting
- [ ] React UI + full Docker stack

## License

Private — portfolio / learning project.
