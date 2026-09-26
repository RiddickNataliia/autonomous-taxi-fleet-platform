# NovaDrive — Autonomous Taxi Fleet Platform
[![C#](https://img.shields.io/badge/C%23-.NET-239120?logo=csharp&logoColor=white)](#)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?logo=dotnet&logoColor=white)](#)
[![Next.js](https://img.shields.io/badge/Frontend-Next.js-black?logo=next.js&logoColor=white)](#)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-4169E1?logo=postgresql&logoColor=white)](#)
[![MongoDB](https://img.shields.io/badge/Database-MongoDB-47A248?logo=mongodb&logoColor=white)](#)
[![GraphQL](https://img.shields.io/badge/API-HotChocolate%20GraphQL-E10098?logo=graphql&logoColor=white)](#)
[![gRPC](https://img.shields.io/badge/Transport-gRPC-244c5a)](#)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](#)

NovaDrive is a full-stack platform for operating a fleet of autonomous taxis: a .NET backend that ingests live vehicle telemetry, matches passengers to rides, handles billing, and exposes REST, GraphQL, and gRPC APIs, plus two Next.js frontends — a passenger app and an admin/ops dashboard — and a vehicle simulator for local development.

## About this project

NovaDrive was built as an individual exam project for my Bachelor's in 
Applied Computer Science (Howest, MCT) — the full backend, both frontends, 
and the CI/CD pipeline were designed and implemented solo. The repository 
was mirrored here (with full commit history preserved) from the 
university-hosted original so it stays visible after the course ends.

## Architecture

The backend follows a clean/layered architecture:

```
NovaDrive/
├── src/
│   ├── NovaDrive.Api             # ASP.NET Core host — REST endpoints, GraphQL (HotChocolate), gRPC
│   ├── NovaDrive.Application     # Services, DTOs, mappings, FluentValidation validators
│   ├── NovaDrive.Domain          # Entities, enums, core domain logic
│   └── NovaDrive.Infrastructure  # EF Core (PostgreSQL), MongoDB, email, PDF invoicing, repositories
├── simulator/                    # Standalone console app that simulates a fleet of vehicles
├── tests/NovaDrive.Tests/        # Unit + integration tests
└── compose.yaml                  # Full local stack (API, DBs, simulator, both frontends)

frontend/
├── admin/                        # Next.js ops dashboard (fleet map, telemetry, discounts, support)
└── passenger/                    # Next.js passenger-facing app (book/track rides, support, profile)
```

**Backend:** ASP.NET Core (.NET 10), Entity Framework Core over PostgreSQL, MongoDB for telemetry/diagnostics data, gRPC for vehicle telemetry ingestion, GraphQL (HotChocolate) for the admin dashboard, REST for passenger/ride/payment flows, Auth0 (JWT bearer) for authentication, Serilog for logging.

**Frontends:** Next.js 16 / React 19, Apollo Client + GraphQL (admin), Leaflet / React-Leaflet for the live fleet map, Recharts for dashboards, Auth0 React SDK for login, Tailwind CSS.

**Simulator:** a .NET console app that spins up a small fleet of virtual vehicles, streams telemetry over gRPC, occasionally reports sensor diagnostics/faults, and drives rides through their lifecycle end to end — useful for exercising the API without real hardware.

## Core domain

- **Rides** — request → en route → completed/canceled lifecycle, matched to a vehicle and passenger
- **Vehicles** — fleet inventory, maintenance logs, live telemetry (location, speed, battery) and sensor diagnostics (LiDAR, radar, camera)
- **Passengers & users** — passenger, admin, and vehicle-system roles
- **Billing** — transactions, discount codes (percentage/flat), PDF invoice generation, multi-currency (EUR/USD) and multiple payment methods
- **Support** — tickets with priority (low → critical) and status tracking

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+
- [Docker](https://www.docker.com/) (for the full local stack)
- An [Auth0](https://auth0.com/) tenant (for authenticated flows)

## Running locally

### Option 1: full stack with Docker Compose

From `NovaDrive/`:

```bash
docker compose up --build
```

This starts:

| Service | URL | Description |
|---|---|---|
| API (REST) | http://localhost:8080 | HTTP/1.1 REST + GraphQL endpoint |
| API (gRPC) | http://localhost:8081 | HTTP/2 telemetry ingestion |
| Admin dashboard | http://localhost:3000 | Next.js ops UI |
| Passenger app | http://localhost:3001 | Next.js passenger UI |
| PostgreSQL | localhost:5432 | Core relational data |
| MongoDB | localhost:27017 | Telemetry / diagnostics |
| Mongo Express | http://localhost:8082 | Mongo admin UI (user/pass: `admin`/`admin`) |
| Mailpit | http://localhost:8025 | Captures outgoing emails in dev |
| Simulator | — | Simulates vehicles, drives itself once the API is healthy |

### Option 2: run services individually

**API**

```bash
cd NovaDrive
dotnet restore NovaDrive.slnx
dotnet run --project src/NovaDrive.Api
```

Requires PostgreSQL and MongoDB connection strings and Auth0 domain/audience to be configured (see `appsettings.json` / `appsettings.Development.json`).

**Simulator**

```bash
cd NovaDrive
dotnet run --project simulator
```

**Frontends**

```bash
cd frontend/admin      # or frontend/passenger
npm install
npm run dev
```

## Testing

```bash
cd NovaDrive
dotnet test NovaDrive.slnx --filter "FullyQualifiedName~UnitTests"
dotnet test NovaDrive.slnx --filter "FullyQualifiedName~IntegrationTests"
```

## CI/CD

GitHub Actions (`.github/workflows/ci.yml`) restores, builds, and runs unit + integration tests on every push/PR to `main`. On pushes to `main`, it additionally builds and publishes the API and simulator Docker images to Docker Hub.

## API surface

- **REST** — passengers, rides, vehicles, payments, discount codes, support tickets, sensor diagnostics, telemetry
- **GraphQL** (`/graphql`) — filtered/sorted queries for the admin dashboard, backed by HotChocolate
- **gRPC** — high-throughput telemetry ingestion from vehicles

## License

No license has been specified for this repository yet.
