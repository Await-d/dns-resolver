# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY src/DnsResolver.Domain/DnsResolver.Domain.csproj ./DnsResolver.Domain/
COPY src/DnsResolver.Application/DnsResolver.Application.csproj ./DnsResolver.Application/
COPY src/DnsResolver.Infrastructure/DnsResolver.Infrastructure.csproj ./DnsResolver.Infrastructure/
COPY src/DnsResolver.Api/DnsResolver.Api.csproj ./DnsResolver.Api/

RUN dotnet restore ./DnsResolver.Api/DnsResolver.Api.csproj

# Copy all source files
COPY src/ ./

# Build the application
WORKDIR /src/DnsResolver.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DnsResolver.Api.dll"]
