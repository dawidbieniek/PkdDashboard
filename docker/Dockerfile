FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the entire solution
COPY . .
RUN dotnet restore "PkdDashboard.AppHost/PkdDashboard.AppHost.csproj"

# Build specific projects
RUN dotnet publish "PkdDashboard.Migrator/PkdDashboard.Migrator.csproj" -c Release -o /app/migrator
RUN dotnet publish "PkdDashboard.WebApp/PkdDashboard.WebApp.csproj" -c Release -o /app/webapp

# Runtime images
# Migrator is referencing webapp, so it needs aspnet (transitive dependency)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS pkd-migrator
WORKDIR /app
COPY --from=build /app/migrator .
ENTRYPOINT ["dotnet", "PkdDashboard.Migrator.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS pkd-dashboard
WORKDIR /app
COPY --from=build /app/webapp .
ENTRYPOINT ["dotnet", "PkdDashboard.WebApp.dll"]
