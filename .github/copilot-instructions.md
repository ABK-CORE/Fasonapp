# Copilot instructions (Fason)

## Big picture

- This repo is a monorepo: **.NET 8 Web API** + **Angular 16 SPA**.
- Backend is N-layer: `WebApi/` (API) → `Business/` (Managers) → `DataAccess/` (EF + Dapper) → `Entities/` (Entities/DTOs) with shared infra in `Core/`, `Infrastructure/`, `Shared/`.
- Frontend lives in `FasonFrontend/` and talks to the backend via `src/environments/*` `apiBaseUrl`.

## Local run (Windows)

- One-command workflow: `start-all.bat` / `stop-all.bat` (logs: `logs/backend.log`, `logs/frontend.log`).
- Manual:
  - Backend: `dotnet run --project WebApi` (HTTPS typically `7047`; HTTP dev port may be `5109`).
  - Frontend: `cd FasonFrontend` then `npm start` (Angular dev server `4200`).
- Script sanity checks: `test-scripts.bat`.

## Backend conventions that matter

- **DI container is Autofac**: add/adjust registrations in `Business/DependencyRepository/Autofac/AutofacBusinessModule.cs`.
- **Config values are encrypted**: `WebApi/appsettings.json` stores encrypted `ConnectionStrings:DatabaseConnection` and `JwtSettings:SecurityKey`; `WebApi/Program.cs` decrypts via `Enigma` + `AesSettings`. Don’t “simplify” by hardcoding plaintext.
- **Auth defaults to required**:
  - `WebApi/Program.cs` adds an `AuthorizeFilter` requiring authenticated users.
  - Most controllers inherit `WebApi/Controllers/BaseApiController.cs` (exposes `CurrentUserGuid` from JWT claims).
  - Use `[AllowAnonymous]` explicitly for public endpoints (e.g., login).
- **Authorization model is layered**:
  - JWT role checks via `User.IsInRole(...)` (example: `WebApi/Controllers/TenderController.cs`).
  - DB-backed permission checks via `[RequireDbRole("...")]` from `Attributes/PermControlAttribute.cs`.
- **Result/response shape**: backend uses `Core/Utilities/Result/*` (`SuccessDataResult`, `ErrorDataResult`, etc.). Controllers usually `return Ok(result)` even on failure.
  - There is an exception middleware implementation in `Infrastructure/Middleware/ExceptionHandlingMiddleware.cs` (not currently wired in `WebApi/Program.cs`); if enabled, it returns HTTP 200 with an `ErrorDataResult`.
  - Keep this contract when adding endpoints (frontend expects `{ success, message, data }`).
- **Swagger is DEBUG-only** and served under `/{ProjectSettings:ProjectName}` (commonly `/fason/index.html`) with Basic Auth (`Infrastructure/Middleware/SwaggerBasicAuthMiddleware.cs`).
- **File uploads**: `ContractManager` writes to `MainFile/` under the app root; `WebApi/Program.cs` serves it at `/MainFile`. Reuse this pathing when adding new file features.

## Frontend conventions that matter

- API calls go through `FasonFrontend/src/app/services/api.service.ts` and expect the backend’s `/api/...` routes.
- Auth is localStorage-based (`token`, `user`):
  - Header injection in `FasonFrontend/src/app/interceptors/auth.interceptor.ts`.
  - Route protection/role gating in `FasonFrontend/src/app/guards/auth.guard.ts`.
- If you change backend ports/URLs, update `FasonFrontend/src/environments/environment.ts` and `WebApi/appsettings.json` CORS (`Cors:Origins`).
