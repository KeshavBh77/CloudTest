# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (layer cache: only reinstall packages when .csproj changes)
COPY ["src/CloudTest/CloudTest.csproj", "src/CloudTest/"]
COPY ["tests/CloudTest.Tests/CloudTest.Tests.csproj", "tests/CloudTest.Tests/"]
RUN dotnet restore "src/CloudTest/CloudTest.csproj"

# Copy everything and build
COPY . .
WORKDIR /src/src/CloudTest
RUN dotnet build "CloudTest.csproj" -c Release -o /app/build

# ── Stage 2: Test ─────────────────────────────────────────────────────────────
FROM build AS test
WORKDIR /src
RUN dotnet test "tests/CloudTest.Tests/CloudTest.Tests.csproj" \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory /testresults \
    --logger "trx;LogFileName=testresults.trx" \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=/testresults/coverage/ \
    /p:Threshold=80

# ── Stage 3: Publish ──────────────────────────────────────────────────────────
FROM build AS publish
WORKDIR /src/src/CloudTest
RUN dotnet publish "CloudTest.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ── Stage 4: Runtime (final image) ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CloudTest.dll"]
