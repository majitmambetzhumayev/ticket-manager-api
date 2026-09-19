# Ticket Manager

**Live demo**: [ticket-manager-api.bluesand-cf8bc58d.francecentral.azurecontainerapps.io](https://ticket-manager-api.bluesand-cf8bc58d.francecentral.azurecontainerapps.io)
(frontend, API, AI classification and search all run live on Azure Container Apps;
sign in with GitHub to create or edit tickets, reads are public)

A support ticket management system built to demonstrate Clean Architecture, CQRS,
AI integration, and a real cloud deployment end to end: a .NET 10 backend, a Python
microservice running a LangGraph classification pipeline, and a React frontend.
Instrumented with OpenTelemetry (traces and metrics), with Redis-backed distributed
rate limiting and Elasticsearch-backed full text search.

When a ticket is created, it is classified by an LLM (priority, category), and if it's
urgent, the system retrieves similar past resolved tickets by embedding similarity and
drafts a suggested response grounded in that history.

## Stack

| Layer            | Tech                                                             |
|-------------------|-------------------------------------------------------------------|
| API               | .NET 10, ASP.NET Core Web API, MediatR (CQRS), FluentValidation   |
| Persistence       | PostgreSQL, Entity Framework Core                                 |
| Search            | Elasticsearch                                                     |
| Rate limiting     | Redis (atomic Lua script, correct under multi-replica load)       |
| Auth              | Azure Container Apps Easy Auth (GitHub OAuth)                     |
| AI service        | Python, FastAPI, LangGraph, OpenAI (classification + embeddings)  |
| Frontend          | React, TypeScript, Vite, Tailwind CSS                             |
| Tests             | xUnit, Testcontainers (.NET, incl. Postgres + Redis) / pytest      |
| Observability     | OpenTelemetry (traces, metrics), Grafana, Prometheus, Tempo       |
| Infra             | Docker, Kubernetes (minikube), Azure Container Apps, GitHub Actions |

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
TicketManager.Infrastructure/  EF Core, repository, AI classifier client, search index
TicketManager.API/             Controllers, DI wiring, middleware, static frontend files
TicketManager.Tests/           Unit tests (Domain) + integration tests (Testcontainers)
ai-service/                    FastAPI app running the LangGraph classification graph
frontend/                      React + TypeScript UI, bundled into the API's image
```

`ai-service` is the only real process/network boundary in the system. The four .NET
projects are a modular monolith: separate assemblies enforcing the dependency rule at
compile time, but a single running process. The frontend is a static SPA served by the
API itself (`wwwroot`), so the whole app is one deployable unit and one origin.

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

## Authentication

Mutating endpoints (`POST`/`PUT`/`DELETE` on `/api/tickets`) are gated behind Azure
Container Apps' built-in authentication (Easy Auth) with GitHub as the identity
provider. Reads stay public: `ai-service`'s own server-to-server call back to the API
(`GET /api/tickets?status=Resolved`, used for the RAG step) can't complete an
interactive GitHub login, and this app has no real per-user data to protect on reads.

Easy Auth runs in "allow unauthenticated" mode so it can inject an
`X-MS-CLIENT-PRINCIPAL-ID` header on requests from a signed-in caller without
blocking everything else; a small middleware (`RequireAuthForMutationsMiddleware`)
checks for that header on mutating verbs only, and no-ops entirely in local
development where Easy Auth doesn't exist.

## Rate limiting

Per-IP rate limiting (100 requests/minute) via a Redis-backed fixed-window counter,
using an atomic Lua script (`INCR` + conditional `EXPIRE`) so the check-and-increment
can't race under concurrent requests. This replaced an earlier in-memory limiter that
had a real bug: with multiple Kubernetes replicas, each pod kept its own independent
counter, silently multiplying the effective limit by the replica count. Fails open if
Redis is unreachable, same resilience stance as the AI classifier below.

## Search

Full text search over ticket title/description via `GET /api/tickets/search?q=`.
The Elasticsearch index only stores title and description, never the full ticket:
search returns matching IDs, and the API re-fetches current data from PostgreSQL,
which stays the single source of truth. Fails open (returns no results rather than an
error) if Elasticsearch is unreachable.

## Observability

The API and ai-service are both instrumented with OpenTelemetry: traces (ASP.NET Core,
HttpClient, EF Core, FastAPI, httpx) and metrics (ASP.NET Core, exposed at `/metrics`).
A request that starts in the API and calls into ai-service stays a single distributed
trace, thanks to W3C traceparent propagation.

Locally, traces go to Tempo, metrics are scraped by Prometheus, and Grafana visualizes
both (datasources are auto-provisioned, no manual setup). Grafana is at
[localhost:3000](http://localhost:3000) once the stack is running. This stack is local
only, not deployed to Azure.

## Kubernetes

`k8s/` holds hand-written `Deployment`/`Service` manifests for the API and its Redis
dependency, run locally via minikube. Secrets (the Postgres connection string, the
GHCR pull credential) are created imperatively with `kubectl create secret` and never
committed. Health probes, `kubectl scale`, rolling updates via
`kubectl rollout restart`, and pod self-healing after a manual `kubectl delete pod`
were all exercised directly, not just declared. The
multi-replica rate limiting bug above was actually found and verified fixed this way,
comparing behavior against the K8s `Service` (which load-balances across pods) before
and after the Redis-backed rewrite.

Kept as a local demo (minikube), not deployed to a managed cluster like AKS, to avoid
paying for a worker node with no scale-to-zero, unlike Azure Container Apps.

## CI/CD

`.github/workflows/ci-cd.yml` runs `dotnet test` and `pytest` in parallel on every push
and pull request, then builds and deploys the API image to Azure Container Apps on
every push to `main`. This only covers the main API: Redis, Elasticsearch, and
`ai-service` were provisioned and are updated manually via the Azure CLI, not part of
the automated pipeline yet.

## Running it

### Full stack with Docker Compose

```bash
cp .env.example .env        # set OPENAI_API_KEY
docker compose up --build
```

The `api` service builds the same image used in production: the frontend is bundled
in, so the whole app (UI + API) is at `http://localhost:8081`.

| Service       | URL                     |
|----------------|--------------------------|
| App (UI + API) | http://localhost:8081    |
| ai-service     | http://localhost:8000    |
| Postgres       | localhost:15432          |
| Redis          | localhost:16379          |
| Elasticsearch  | http://localhost:9200    |
| Grafana        | http://localhost:3000    |
| Prometheus     | http://localhost:9090    |
| Tempo          | http://localhost:3200    |

### Backend only, local dev

```bash
docker compose up db redis elasticsearch -d
dotnet ef database update \
  --project TicketManager.Infrastructure --startup-project TicketManager.API
dotnet run --project TicketManager.API     # http://localhost:5020
```

Redis and Elasticsearch are optional here: both fail open, so rate limiting and
search just degrade (no throttling, empty search results) if they're not running.

### Frontend, local dev (with hot reload)

```bash
cd frontend
cp .env.example .env        # points at http://localhost:5020
npm install
npm run dev                 # http://localhost:5173
```

## API

| Method | Route                              | Auth       | Description                                    |
|--------|--------------------------------------|------------|--------------------------------------------------|
| POST   | `/api/tickets`                      | required   | Create a ticket (triggers AI classification)      |
| GET    | `/api/tickets`                      | public     | List tickets, optional `?status=` filter          |
| GET    | `/api/tickets/{id}`                  | public     | Get a ticket by id                                |
| GET    | `/api/tickets/search?q=`             | public     | Full text search over title/description           |
| PUT    | `/api/tickets/{id}`                  | required   | Update title/description (Open/InProgress only)   |
| POST   | `/api/tickets/{id}/start-progress`  | required   | Open -> InProgress                                |
| POST   | `/api/tickets/{id}/resolve`         | required   | InProgress -> Resolved, with resolution notes      |
| POST   | `/api/tickets/{id}/close`           | required   | Resolved -> Closed                                |
| DELETE | `/api/tickets/{id}`                  | required   | Delete (Open tickets only)                        |
| GET    | `/api/auth/status`                   | public     | `{ isSignedIn: bool }`, used by the frontend        |

## Testing

```bash
dotnet test                 # unit tests + integration tests (needs Docker running)
```

Integration tests spin up real, ephemeral Postgres and Redis containers via
Testcontainers, not in-memory fakes: the in-memory EF Core provider doesn't enforce
real constraints or translate LINQ the way Postgres does, which is exactly the kind of
gap that produces a green test suite and a broken production query.

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
