# Ticket Manager

**Live API**: [ticket-manager-api.bluesand-cf8bc58d.francecentral.azurecontainerapps.io/api/tickets](https://ticket-manager-api.bluesand-cf8bc58d.francecentral.azurecontainerapps.io/api/tickets)
(deployed on Azure Container Apps; the AI classification service and frontend run locally only,
see below)

A support ticket management system built to demonstrate Clean Architecture, CQRS, and
AI integration end to end: a .NET 10 backend, a Python microservice running a LangGraph
classification pipeline, and a React frontend. Instrumented with OpenTelemetry (traces and
metrics) with a local Grafana/Prometheus/Tempo stack for observability.

When a ticket is created, it is classified by an LLM (priority, category), and if it's
urgent, the system retrieves similar past resolved tickets by embedding similarity and
drafts a suggested response grounded in that history.

## Stack

| Layer          | Tech                                                             |
|----------------|-------------------------------------------------------------------|
| API            | .NET 10, ASP.NET Core Web API, MediatR (CQRS), FluentValidation   |
| Persistence    | PostgreSQL, Entity Framework Core                                 |
| AI service     | Python, FastAPI, LangGraph, OpenAI (classification + embeddings)  |
| Frontend       | React, TypeScript, Vite, Tailwind CSS                             |
| Tests          | xUnit, Testcontainers (.NET) / pytest (ai-service)                |
| Observability  | OpenTelemetry (traces, metrics), Grafana, Prometheus, Tempo       |
| Infra          | Docker Compose, Azure Container Apps                              |

## Architecture

The backend follows Clean Architecture with a strict dependency rule:

```
API -> Infrastructure -> Application -> Domain
```

`Domain` has no external dependencies. `Application` depends only on `Domain`.
`Infrastructure` implements the ports `Application`/`Domain` define. `API` wires
everything together via dependency injection.

```
TicketManager.Domain/          Entities, enums, domain exceptions, repository interface
TicketManager.Application/     Commands/Queries (MediatR), DTOs, validators
TicketManager.Infrastructure/  EF Core, repository implementation, AI classifier client
TicketManager.API/             Controllers, DI wiring, global exception handling
TicketManager.Tests/           Unit tests (Domain) + integration tests (Testcontainers)
ai-service/                    FastAPI app running the LangGraph classification graph
frontend/                      React + TypeScript UI
```

`ai-service` is the only real process/network boundary in the system. The four .NET
projects are a modular monolith: separate assemblies enforcing the dependency rule at
compile time, but a single running process.

### AI classification graph

```
classify -> (if priority is High or Critical) -> retrieve_similar -> suggest_response
         -> (otherwise) -> end
```

`retrieve_similar` embeds the new ticket and compares it against resolved tickets
fetched from the API, keeping only matches above a similarity threshold. If no similar
ticket is found, the graph still drafts a best-effort suggestion but flags it as
`grounded_in_history: false` rather than implying it's based on precedent that doesn't
exist.

## Observability

The API and ai-service are both instrumented with OpenTelemetry: traces (ASP.NET Core,
HttpClient, EF Core, FastAPI, httpx) and metrics (ASP.NET Core, exposed at `/metrics`).
A request that starts in the API and calls into ai-service stays a single distributed
trace, thanks to W3C traceparent propagation.

Locally, traces go to Tempo, metrics are scraped by Prometheus, and Grafana visualizes
both (datasources are auto-provisioned, no manual setup). Grafana is at
[localhost:3000](http://localhost:3000) once the stack is running.

## Running it

### Full stack with Docker Compose

```bash
cp .env.example .env        # set OPENAI_API_KEY
docker compose up --build
```

| Service    | URL                     |
|------------|--------------------------|
| API        | http://localhost:8081    |
| ai-service | http://localhost:8000    |
| Postgres   | localhost:15432          |
| Grafana    | http://localhost:3000    |
| Prometheus | http://localhost:9090    |
| Tempo      | http://localhost:3200    |

The frontend isn't containerized yet; run it separately (see below) against the API.

### Backend only, local dev

```bash
docker compose up db -d                    # Postgres only
dotnet ef database update \
  --project TicketManager.Infrastructure --startup-project TicketManager.API
dotnet run --project TicketManager.API     # http://localhost:5020
```

### Frontend, local dev

```bash
cd frontend
cp .env.example .env        # defaults to http://localhost:5020
npm install
npm run dev                 # http://localhost:5173
```

## API

| Method | Route                          | Description                                 |
|--------|----------------------------------|----------------------------------------------|
| POST   | `/api/tickets`                  | Create a ticket (triggers AI classification)  |
| GET    | `/api/tickets`                  | List tickets, optional `?status=` filter      |
| GET    | `/api/tickets/{id}`             | Get a ticket by id                            |
| PUT    | `/api/tickets/{id}`              | Update title/description (Open tickets only)  |
| POST   | `/api/tickets/{id}/start-progress` | Open -> InProgress                         |
| POST   | `/api/tickets/{id}/resolve`     | InProgress -> Resolved, with resolution notes |
| POST   | `/api/tickets/{id}/close`       | Resolved -> Closed                            |
| DELETE | `/api/tickets/{id}`              | Delete (Open tickets only)                    |

## Testing

```bash
dotnet test                 # unit tests + integration tests (needs Docker running)
```

```bash
cd ai-service
pip install -e ".[dev]"
pytest
```

```bash
cd frontend
npm run lint
npm run build               # runs the TypeScript compiler in strict mode
```
