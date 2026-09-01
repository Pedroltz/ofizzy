FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/backend/SportPneus.Api/SportPneus.Api.csproj src/backend/SportPneus.Api/
RUN dotnet restore src/backend/SportPneus.Api/SportPneus.Api.csproj
COPY src/backend/SportPneus.Api src/backend/SportPneus.Api
RUN dotnet publish src/backend/SportPneus.Api/SportPneus.Api.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SportPneus.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /workspace
ENV ASPNETCORE_URLS=http://+:8080 DOTNET_USE_POLLING_FILE_WATCHER=1
EXPOSE 8080
CMD ["dotnet", "watch", "--project", "src/backend/SportPneus.Api", "run", "--no-launch-profile"]
