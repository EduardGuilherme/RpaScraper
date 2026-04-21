# ── Stage 1: build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Copy nuget config first — clears Windows fallback paths before any restore
COPY nuget.config ./

COPY RpaScraper.sln ./
COPY src/Rpa.Domain/Rpa.Domain.csproj               src/Rpa.Domain/
COPY src/Rpa.Infrastructure/Rpa.Infrastructure.csproj src/Rpa.Infrastructure/
COPY src/Rpa.Api/Rpa.Api.csproj                     src/Rpa.Api/
COPY src/Rpa.Worker/Rpa.Worker.csproj               src/Rpa.Worker/

# Restore the full solution so all transitive packages are downloaded
RUN dotnet restore RpaScraper.sln --configfile nuget.config

COPY src/ src/

# Publish without --no-restore so MSBuild can resolve any remaining assets
RUN dotnet publish src/Rpa.Worker/Rpa.Worker.csproj \
    -c Release \
    -o /app/publish

# ── Stage 2: runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Rpa.Worker.dll"]
