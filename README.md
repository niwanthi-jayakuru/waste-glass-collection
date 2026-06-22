# Waste Glass Collection App

Intern assignment: Flutter Android app + .NET REST API for waste glass collection route planning and barcode-gated collection.

## Project Structure

```
Glass Collection Driver/
├── backend/
│   └── GlassCollectionApi/    # .NET 8 Web API
├── mobile/                    # Flutter app (Phase 5+)
├── docs/
│   └── DATABASE.md            # Schema & seed data reference
└── README.md
```

## Database Choice

**Development:** SQLite via Entity Framework Core (file: `glasscollection.db`).

**Production (planned):** PostgreSQL on Supabase + API on Railway/Render.

### Justification

| Requirement | Why SQLite (dev) + PostgreSQL (prod) |
|-------------|--------------------------------------|
| No existing backend | EF Core supports both with the same schema |
| Hosted evaluation | PostgreSQL on Supabase free tier is always online |
| Relational data | Suppliers, trips, and stops fit a relational model |
| Offline app | Mobile uses its own SQLite; server DB is for sync/API |
| Easy seeding | `DbInitializer` seeds test suppliers on first run |

SQLite keeps local development simple (no Docker). PostgreSQL on Supabase gives a persistent hosted database for the APK and evaluators.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Flutter SDK](https://docs.flutter.dev/get-started/install) (for mobile, later phases)

## Backend Setup (Phase 1–2)

```powershell
cd backend/GlassCollectionApi
dotnet restore
dotnet ef database update
dotnet run
```

API runs at `https://localhost:7xxx` (see console output). Swagger UI is available in Development.

### Connection String

Default in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=glasscollection.db"
}
```

For PostgreSQL later, swap to:

```json
"DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
```

and change `UseSqlite` to `UseNpgsql` in `Program.cs`.

## Seed Data

On first run, the API seeds **5 suppliers** (IDs `SUP001`–`SUP005`) and **today's trip** with stops in pending state.

Generate Code 128 barcodes for each supplier ID at [barcode.tec-it.com](https://barcode.tec-it.com) for testing.

See [docs/DATABASE.md](docs/DATABASE.md) for full schema.

## Phases

| Phase | Status | Description |
|-------|--------|-------------|
| 1 | Done | Project structure, README, backend scaffold |
| 2 | Done | Database schema, EF models, seed data |
| 3 | Pending | .NET API endpoints + Dijkstra routing |
| 4 | Pending | Barcode test assets |
| 5 | Pending | Flutter 3 screens |
| 6 | Pending | Deploy + APK + submission |

## GitHub

Push this repo to GitHub before submission. Do not commit `glasscollection.db` (see `.gitignore`).
