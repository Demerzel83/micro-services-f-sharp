# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["catalog-service/api/catalog-service-api.fsproj", "catalog-service/api/"]
COPY ["catalog-service/core/catalog-service-core.fsproj", "catalog-service/core/"]
COPY ["utils/custom-types/custom-types.fsproj", "utils/custom-types/"]
COPY ["catalog-service/infrastructure/catalog-service-infrastructure.fsproj", "catalog-service/infrastructure/"]
RUN dotnet restore "./catalog-service/api/catalog-service-api.fsproj"
COPY . .
WORKDIR "/src/catalog-service/api"
RUN dotnet build "./catalog-service-api.fsproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./catalog-service-api.fsproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "catalog_service.dll"]