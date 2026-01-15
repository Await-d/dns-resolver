# Stage 1: Build frontend
FROM node:22-alpine AS frontend-build
WORKDIR /app/frontend

# Copy package files
COPY frontend/package.json frontend/pnpm-lock.yaml ./

# Install pnpm and dependencies
RUN npm install -g pnpm && pnpm install --frozen-lockfile

# Copy source files
COPY frontend/ .

# Build the frontend application
RUN pnpm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY backend/src/DnsResolver.Domain/DnsResolver.Domain.csproj ./DnsResolver.Domain/
COPY backend/src/DnsResolver.Application/DnsResolver.Application.csproj ./DnsResolver.Application/
COPY backend/src/DnsResolver.Infrastructure/DnsResolver.Infrastructure.csproj ./DnsResolver.Infrastructure/
COPY backend/src/DnsResolver.Api/DnsResolver.Api.csproj ./DnsResolver.Api/

RUN dotnet restore ./DnsResolver.Api/DnsResolver.Api.csproj

# Copy all source files
COPY backend/src/ ./

# Build and publish the application
WORKDIR /src/DnsResolver.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published backend files
COPY --from=backend-build /app/publish .

# Copy frontend build output to wwwroot
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DnsResolver.Api.dll"]
