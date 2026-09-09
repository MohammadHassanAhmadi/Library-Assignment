# Library API System

An HTTP API over a gRPC service. It answers four reporting questions for a public library:
which books get borrowed most, who borrows most in a period, how fast a borrower reads,
and what else the readers of a book borrowed.

## Prerequisites

- .NET 8 SDK
- SQL Server

## 1. Database

**If you already have SQL Server**, point these two connection strings at it:

| File | Key |
|---|---|
| `src/Library.Service/appsettings.json` | `LibraryDb` |
| `test/Library.IntegrationTests/appsettings.json` | `LibraryDbTest` |

**If you don't have one**, or would rather not install anything, start one with Docker:

```bash
docker compose up -d
```

This matches the connection strings already in the repo, so there is nothing to change.

Either way, the database is created and filled with sample data when the service starts.
There are no SQL scripts to run.

## 2. Run

```bash
run.bat
```

This opens the gRPC service and the API in two windows, then opens Swagger at
<http://localhost:5255/swagger>.

To run them by hand instead, use two terminals:

```bash
dotnet run --project src/Library.Service
```

```bash
dotnet run --project src/Library.Api
```

## 3. Tests

```bash
dotnet test
```

59 unit tests and 33 integration tests. The integration tests create and drop their own
database (`LibraryDb_Tests`), so the connection string above is all they need.

## Endpoints

| Endpoint | Question |
|---|---|
| `GET /api/reports/most-borrowed-books?limit=10` | What are the most borrowed books? |
| `GET /api/reports/most-active-borrowers?from=&to=&limit=10` | Who borrowed most in a period? |
| `GET /api/reports/loans/{loanId}/reading-pace` | How fast did a borrower read a book? |
| `GET /api/reports/books/{bookId}/readers-also-borrowed?limit=10` | What else did this book's readers borrow? |

Dates are ISO-8601, for example `2026-02-01T00:00:00Z`.

## Projects

| Project | Role |
|---|---|
| `Library.Api` | HTTP endpoints, error mapping, Swagger |
| `Library.Service` | gRPC service, EF Core, report queries |
| `Library.Contracts` | the `.proto` contract shared by both |
| `Library.Warmups` | the four warm-up exercises |

The API has no database access. Every report is answered by the service over gRPC.

## Notes

- Errors are returned as ProblemDetails with a `traceId`. The same id appears in the log
  files under `logs/`, so a failed response can be traced to its log line.
- "Most active borrowers" counts **transactions**, not distinct books. Borrowing the same
  book twice counts as two.