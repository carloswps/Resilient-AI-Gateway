# AGENTS.md — Resilient-AI-Gateway

## Current state: early scaffold

The repo contains a single scaffolded ASP.NET Core 10 project (the default weather forecast template). **`Guia_Desenvolvimento.md` is the architectural vision, not the current implementation.** The elaborate directory tree, Polly pipeline, MongoDB logging, Nginx edge proxy, Shared contracts project, and test projects described there do not exist yet.

## Real entrypoints and structure

| Truth | File |
|---|---|
| App entrypoint | `Resilient-AI-Gateway/Program.cs` (20 lines, default template) |
| Only dependency | `Microsoft.AspNetCore.OpenApi` v10.0.3 |
| Project file | `Resilient-AI-Gateway/Resilient-AI-Gateway.csproj` (net10.0) |
| Solution | `Resilient-AI-Gateway.sln` — single project |
| Docker Compose | `compose.yaml` (modern v2 format, NOT `docker-compose.yml`) |
| Dockerfile | `Resilient-AI-Gateway/Dockerfile` (multi-stage, exposes 8080/8081) |
| Dev server URL | `http://localhost:5243` |

## Build and run commands

```bash
# Build
dotnet build

# Run locally
dotnet run --project Resilient-AI-Gateway/Resilient-AI-Gateway.csproj
# Serves on http://localhost:5243

# Docker
docker compose up --build

# Test endpoint (as defined in .http file)
curl http://localhost:5243/weatherforecast/
```

## What does NOT exist (yet)

- No Polly, MongoDB.Driver, or any package beyond OpenAPI
- No test projects or test framework references
- No Nginx config, no `nginx/` directory
- No `mongo/` directory, no MongoDB init script
- No `docs/` directory, no ADRs
- No `.env` or `.env.example`
- No CI workflows, no pre-commit, no lint config
- No `ResilientAIGateway.Shared` project
- No custom middleware, services, models, or endpoints

## Toolchain notes

- .NET SDK **10.0** required (preview/RC — verify `dotnet --list-sdks`)
- `.http` file uses `@Resilient_AI_Gateway_HostAddress` variable set to `http://localhost:5243`
- Build artifacts go to `Resilient-AI-Gateway/bin/` and `Resilient-AI-Gateway/obj/` (in .gitignore)
- The `Guia_Desenvolvimento.md` is the design document — consult it as the target architecture when implementing features
- Root namespace: `Resilient_AI_Gateway` (underscores, NOT dots as in folder name)
- Solution file references `compose.yaml` as a Solution Item
