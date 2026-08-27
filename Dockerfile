# syntax=docker/dockerfile:1

# The project targets netcoreapp3.1, but builds with the 5.0 SDK on purpose: the NuGet client that
# ships with the 3.1 SDK is too old to restore the current MySqlConnector package.
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /src

# Restore in its own layer, so editing a source file does not re-download every package.
COPY WebStats/WebStats.csproj WebStats/
RUN dotnet restore WebStats/WebStats.csproj

COPY . ./
RUN dotnet publish WebStats/WebStats.csproj -c Release -o /app/out --no-restore

# Build runtime image.
# Alpine rather than the Debian default: busybox ships the wget used by the health check below.
# The Debian buster image has neither wget nor curl, and buster apt is archived.
FROM mcr.microsoft.com/dotnet/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build-env /app/out .

# The base image listens on port 80 by default, which a non-root user may not bind.
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENV DATABASE_HOST=
ENV DATABASE_NAME=
ENV DATABASE_USER=
ENV DATABASE_PASSWORD=

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:5000/health || exit 1

# Nothing is written to disk, so /app stays owned by root and read only to the app.
RUN adduser -D -u 5678 appuser
USER appuser

ENTRYPOINT ["dotnet", "WebStats.dll"]
