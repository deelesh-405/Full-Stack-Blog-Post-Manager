# Fieldnotes Blog Manager

A full-stack blog post manager with an ASP.NET Core API, a React + TypeScript frontend, and SQL Server persistence.

## Stack

- ASP.NET Core 9 Web API
- Entity Framework Core 9 with SQL Server and InMemory providers
- React 19, TypeScript, Vite, React Router, and Axios
- SQL Server 2022 and Docker Compose
- xUnit unit and integration tests

## Run with Docker Compose

1. Copy `.env.example` to `.env` and set `MSSQL_SA_PASSWORD` to a strong local password (SQL Server requires at least 8 characters with uppercase, lowercase, a digit, and a symbol).
2. Start the services with `docker compose up --build`.
3. Open the frontend at [http://localhost:5173](http://localhost:5173). The API and Swagger UI are available at [http://localhost:5020](http://localhost:5020) and [http://localhost:5020/swagger](http://localhost:5020/swagger).

The API waits for SQL Server's health check before starting. Its initial schema is created from the EF Core model; once migrations are added, startup applies them instead. SQL Server data persists in the `sqlserver-data` volume.

## Run locally

The API uses the InMemory provider by default in `appsettings.json`, so no SQL Server is needed for a quick local run:

```powershell
dotnet run --project src/BlogManager.API --urls http://localhost:5020
```

In another terminal:

```powershell
cd frontend
npm ci
npm run dev
```

The Vite app uses `http://localhost:5020/api` by default. Override it with `VITE_API_URL` if the API runs elsewhere. To use SQL Server locally, set `UseInMemoryDatabase` to `false` and configure `ConnectionStrings:DefaultConnection`.

## API

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/posts?page=1&pageSize=10&search=&status=` | List, search, filter, and paginate posts |
| `GET` | `/api/posts/dashboard` | Dashboard counts and recent posts |
| `GET` | `/api/posts/{id}` | Get a post |
| `POST` | `/api/posts` | Create a post |
| `PUT` | `/api/posts/{id}` | Update a post |
| `DELETE` | `/api/posts/{id}` | Delete a post |

Statuses are numeric: `0` for Draft and `1` for Published. Slugs are generated from titles and must be unique.

## Tests and frontend checks

```powershell
dotnet test BlogManager.sln
cd frontend
npm run build
npm run lint
```

The API tests use EF Core InMemory and do not require Docker or SQL Server.