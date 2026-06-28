# Local infrastructure

Postgres and Redis for local development. API and frontend run on the host.

```bash
docker compose up -d
```

Defaults match `src/MultiTenantSaaS.Api/appsettings.json`:

| Service  | Port |
|----------|------|
| Postgres | 5432 |
| Redis    | 6379 |

Stop:

```bash
docker compose down
```
