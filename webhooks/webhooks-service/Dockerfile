#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["webhooks/webhooks-service/webhooks-service.fsproj", "webhooks/webhooks-service/"]
RUN dotnet restore "webhooks/webhooks-service/webhooks-service.fsproj"
COPY . .
WORKDIR "/src/webhooks/webhooks-service"
RUN dotnet build "webhooks-service.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "webhooks-service.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "webhooks-service.dll"]