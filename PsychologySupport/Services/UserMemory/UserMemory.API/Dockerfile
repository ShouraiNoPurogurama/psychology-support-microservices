# ===== Base (runtime) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update \
  && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/* \
  && mkdir -p /app/gcloud /https \
  && chmod 755 /app /app/gcloud /https \
  && if [ -n "$APP_GID" ]; then chown -R "$APP_UID:$APP_GID" /app /https; else chown -R "$APP_UID" /app /https; fi
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "./"]

# (2) Copy các csproj mà UserMemory.API tham chiếu
COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks/BuildingBlocks.Messaging/"]


COPY ["Services/UserMemory/UserMemory.API/UserMemory.API.csproj", "Services/UserMemory/UserMemory.API/"]

RUN dotnet restore "./Services/UserMemory/UserMemory.API/UserMemory.API.csproj"

# (4) Copy source code còn lại rồi build
COPY . .
WORKDIR "/src/Services/UserMemory/UserMemory.API"
RUN dotnet build "./UserMemory.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== Publish =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserMemory.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== Final =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=10s --timeout=3s --retries=6 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "UserMemory.API.dll"]
