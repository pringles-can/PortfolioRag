# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (cached unless project/global.json change)
COPY global.json ./
COPY PortfolioRag.Api/PortfolioRag.Api.csproj PortfolioRag.Api/
RUN dotnet restore PortfolioRag.Api/PortfolioRag.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish PortfolioRag.Api/PortfolioRag.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kestrel listens on 8080 (the .NET 8 non-root default)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PortfolioRag.Api.dll"]
