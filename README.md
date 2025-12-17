# TaskTracker

A modern task tracking API similar to Jira, Linear, and YouTrack, built with ASP.NET Core 8 following Clean Architecture principles.

## Features

- **Clean Architecture** - Domain, Application, Infrastructure, and API layers
- **CQRS Pattern** - Command Query Responsibility Segregation with MediatR
- **PostgreSQL** - Primary database with JSONB support for custom fields
- **Redis** - Distributed caching
- **RabbitMQ** - Message broker with MassTransit
- **SignalR** - Real-time notifications
- **JWT Authentication** - Secure API access with refresh tokens
- **RBAC** - Role-based access control (Owner, Admin, Member, Guest)
- **Workflow Engine** - Customizable task statuses with transitions
- **Audit Log** - Complete history of all task changes

## Tech Stack

- .NET 8 (LTS)
- Entity Framework Core 8 (Code First)
- MediatR 14 (CQRS)
- FluentValidation 12
- Serilog (Structured Logging)
- OpenTelemetry (Metrics and Tracing)
- Swagger/OpenAPI

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker (for local infrastructure)

### Running Infrastructure

Start the local infrastructure using Docker Compose:

```bash
docker-compose up -d
```

This will start:
- PostgreSQL (port 5432)
- Redis (port 6379)
- RabbitMQ (ports 5672, 15672 for management)
- Seq (port 5341 for log viewing)

### Database Migrations

Create initial migration:

```bash
cd src/TaskTracker.Infrastructure
dotnet ef migrations add InitialCreate -s ../TaskTracker.Api
```

Apply migrations:

```bash
cd src/TaskTracker.Api
dotnet ef database update
```

### Running the API

```bash
cd src/TaskTracker.Api
dotnet run
```

The API will be available at:
- Swagger UI: http://localhost:5000
- Health Check: http://localhost:5000/health

## Project Structure

```
TaskTracker/
├── src/
│   ├── TaskTracker.Domain/           # Entities, Value Objects, Domain Events
│   ├── TaskTracker.Application/      # Use Cases, DTOs, CQRS Handlers
│   ├── TaskTracker.Infrastructure/   # EF Core, Repositories, External Services
│   └── TaskTracker.Api/              # Controllers, Middleware, Configuration
├── tests/
│   ├── TaskTracker.Tests.Unit/       # Unit Tests
│   └── TaskTracker.Tests.Integration/ # Integration Tests with Testcontainers
└── docker-compose.yml
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get tokens
- `POST /api/auth/refresh` - Refresh access token
- `GET /api/auth/me` - Get current user profile

### Workspaces
- `GET /api/workspaces` - List all workspaces
- `GET /api/workspaces/{id}` - Get workspace details
- `POST /api/workspaces` - Create workspace
- `PUT /api/workspaces/{id}` - Update workspace
- `DELETE /api/workspaces/{id}` - Delete workspace
- `GET /api/workspaces/{id}/members` - List workspace members

### Projects
- `GET /api/workspaces/{workspaceId}/projects` - List projects
- `GET /api/workspaces/{workspaceId}/projects/{id}` - Get project details
- `POST /api/workspaces/{workspaceId}/projects` - Create project
- `GET /api/workspaces/{workspaceId}/projects/{id}/statuses` - Get workflow statuses

### Tasks
- `GET /api/tasks` - List tasks with filtering and pagination
- `GET /api/tasks/{id}` - Get task details
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `PATCH /api/tasks/{id}/status` - Change task status
- `PATCH /api/tasks/{id}/assign` - Assign task
- `PATCH /api/tasks/{id}/priority` - Change priority

## Running Tests

### Unit Tests

```bash
dotnet test tests/TaskTracker.Tests.Unit
```

### Integration Tests

Integration tests use Testcontainers and require Docker:

```bash
dotnet test tests/TaskTracker.Tests.Integration
```

## Configuration

Key configuration options in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=tasktracker;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "your-secret-key",
    "Issuer": "TaskTracker",
    "Audience": "TaskTrackerClients",
    "ExpiryMinutes": 60
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

## SignalR Hub

Connect to `/hubs/tasks` for real-time updates. Available events:
- `TaskCreated`
- `TaskUpdated`
- `TaskStatusChanged`
- `TaskAssigned`

## License

MIT
