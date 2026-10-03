# Gymnasium 5 - Leaderboard Backend

## Project Overview

REST API for the school leaderboard of Gymnasium 5. Owners (teachers) manage classes and students and award or
deduct points; anyone can view the class and student rankings for the current academic year.

## Technologies and Patterns Used

* **.NET 10 & C#** - Core framework and language
* **ASP.NET Core** - HTTP API with versioning and Swagger
* **Entity Framework Core with PostgreSQL** - Data access (Repository & Unit of Work patterns)
* **JWT + refresh tokens** - Access token in the `Authorization` header, rotating refresh token in an HttpOnly cookie
* **Idempotency** - Score changes accept an `Idempotency-Key` header, so retries never apply points twice
* **FluentValidation, AutoMapper, Serilog** - Validation, mapping and structured logging
* **Clean Architecture** - Layered separation (Domain, Application, Infrastructure, Presentation)
* **xUnit, Moq, Testcontainers & Coverlet** - Unit and functional tests against a real PostgreSQL with code coverage

## Architecture and Design

| Layer              | Project                                                            |
|--------------------|--------------------------------------------------------------------|
| **Presentation**   | Leaderboard.Api                                                    |
| **Application**    | Leaderboard.Application                                            |
| **Domain**         | Leaderboard.Domain                                                 |
| **Infrastructure** | Leaderboard.DAL, Leaderboard.Cache, Leaderboard.BackgroundJobs     |

## Getting Started for developers

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/)

### Installation

1. Clone the repo
2. Create a `.env` file next to `docker-compose.yml`:
   ```dotenv
   POSTGRES_PASSWORD=<YOUR-PASSWORD>
   REDIS_PASSWORD=<YOUR-PASSWORD>
   JWT_SIGNING_KEY=<AT-LEAST-32-BYTES>
   ```
3. Start the dependencies:
   ```bash
   docker compose up -d postgres redis
   ```
4. Set `.NET User Secrets` in `Leaderboard.Api` (never put secrets in `appsettings.json`):
   ```bash
   dotnet user-secrets --project Leaderboard.Api set "ConnectionStrings:PostgresSQL" "Server=localhost;Port=15432;Database=leaderboard;User Id=postgres;Password=<YOUR-PASSWORD>"
   dotnet user-secrets --project Leaderboard.Api set "JwtSettings:SigningKey" "<AT-LEAST-32-BYTES>"
   ```
5. Run the API:
   ```bash
   dotnet run --project Leaderboard.Api
   ```
   or use your IDE. Migrations are applied and the first academic year is created on startup.

To run everything in Docker instead: `docker compose up -d --build` (API on http://localhost:8080).

## API Documentation

Swagger UI is available in Development at http://localhost:5180/swagger.

| Controller        | Route                  | Purpose                                          |
|-------------------|------------------------|--------------------------------------------------|
| Auth              | `/api/auth`            | Login, refresh, logout                           |
| Owner             | `/api/owner`           | Owner accounts and password change               |
| Class             | `/api/class`           | Classes (filter by `?grade=`)                    |
| Student           | `/api/student`         | Students, batch create, transfer, deactivate     |
| Score             | `/api/score`           | Award or deduct points, history                  |
| Leaderboard       | `/api/leaderboard`     | Class and student rankings                       |
| AcademicYear      | `/api/academicyear`    | Current year and starting a new one (graduation) |

## Testing

Functional tests start PostgreSQL in Docker via Testcontainers, so Docker must be running.

```bash
dotnet test --filter Category=Unit
dotnet test --filter Category=Functional
```

## CI

GitHub Actions on push to `master` and on pull requests:

* `tests.yml` - unit and functional tests with Coverlet coverage
* `docker.yml` - builds and pushes the API image to Docker Hub (push to `master` only)

Required repository secrets: `DOCKER_USERNAME`, `DOCKER_PASSWORD`.
