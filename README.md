# CloudTest — ASP.NET Core REST API

**C# · .NET 8 · ASP.NET Core · Entity Framework Core · SQL Server · Docker · GitHub Actions**

A REST API testing framework built with a clean 3-layer architecture: Controllers → Services → Repositories.

---

## Architecture

```
HTTP Request
     │
     ▼
 Controllers          Receive HTTP, validate input, return responses
     │                No business logic. No DB access.
     ▼
  Services            Business logic, entity↔DTO mapping, validation rules
     │
     ▼
 Repositories         Only layer that touches EF/DbContext
     │
     ▼
  AppDbContext         Translates LINQ to SQL via Entity Framework Core
     │
     ▼
  SQL Server           5 normalized tables with indexed FK columns
```

---

## Database Schema (5 Normalized Tables)

```
Users ──────────────────────────────────────────────────────
  Id | Username | Email (unique) | CreatedAt

TestSuites ─────────────────────────────────────────────────
  Id | Name | Description | CreatedAt

TestCases ──────────────────────────────────────────────────
  Id | Title | ExpectedBehavior | CreatedAt | TestSuiteId (FK, indexed)

TestRuns ───────────────────────────────────────────────────
  Id | ExecutedAt | Status | UserId (FK, indexed)

TestResults ────────────────────────────────────────────────
  Id | Status | Notes | ResponseTimeMs | RecordedAt
     | TestCaseId (FK) | TestRunId (FK)
     | Composite index (TestRunId, TestCaseId)
```

---

## Endpoints (10 total)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users |
| GET | `/api/users/{id}` | Get user by ID |
| POST | `/api/users` | Create user |
| DELETE | `/api/users/{id}` | Delete user |
| GET | `/api/testsuites` | Get all suites |
| GET | `/api/testsuites/{id}` | Get suite with test cases (eager loaded) |
| POST | `/api/testsuites` | Create suite |
| PUT | `/api/testsuites/{id}` | Update suite |
| DELETE | `/api/testsuites/{id}` | Delete suite |
| GET | `/api/testsuites/{id}/testcases` | Get cases for a suite |

Plus full CRUD on `/api/testsuites/{id}/testcases`, `/api/testruns`, and `/api/testresults`.

---

## Performance

- **Sub-200ms average response time** via:
  - `async`/`await` on all DB calls — threads never block
  - `AsNoTracking()` on all read queries — skips EF change tracking
  - Indexed FK columns — O(log n) JOIN lookups
  - Eager loading via `.Include()` — one SQL query instead of N+1

---

## Running Locally

### Option 1: Docker Compose (recommended — no SQL Server install needed)

```bash
# Copy env file
cp .env.example .env         # set DB_PASSWORD

# Start everything (SQL Server + API)
docker compose up --build

# API: http://localhost:8080
# Swagger: http://localhost:8080
```

### Option 2: .NET CLI (requires SQL Server or LocalDB)

```bash
cd src/CloudTest

# Set connection string in appsettings.Development.json
dotnet ef database update    # apply migrations

dotnet run
# Swagger: https://localhost:5001
```

---

## Running Tests

```bash
# All tests (xUnit + NUnit)
dotnet test CloudTest.sln

# With coverage report
dotnet test CloudTest.sln \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./coverage/ \
  /p:Threshold=80
```

**Test coverage: 84%** across service and repository layers.

---

## CI/CD Pipeline (GitHub Actions)

| Stage | Trigger | Time |
|-------|---------|------|
| Build + Test (coverage ≥ 80%) | Every push / PR | ~4 min |
| Docker build + push to GHCR | Push to `main` | ~2 min |
| Deploy via SSH | Push to `main` | ~1 min |
| **Total** | | **~7 min** |

Previous manual process: **~40 minutes** (manual build → manual test → manual deploy).

---

## GitHub Secrets Required

| Secret | Description |
|--------|-------------|
| `DB_PASSWORD` | SQL Server SA password |
| `DEPLOY_HOST` | Production server IP/hostname |
| `DEPLOY_USER` | SSH username |
| `DEPLOY_SSH_KEY` | Private SSH key for deployment |

---

## Tech Stack

- **Runtime**: .NET 8 / ASP.NET Core
- **ORM**: Entity Framework Core 8 with SQL Server provider
- **Testing**: xUnit (service layer) + NUnit (repository layer) + Moq
- **Coverage**: coverlet (84% threshold: 80%)
- **Containerization**: Docker multi-stage build
- **CI/CD**: GitHub Actions (build → test → push → deploy)
- **API Docs**: Swagger / OpenAPI at `/`
