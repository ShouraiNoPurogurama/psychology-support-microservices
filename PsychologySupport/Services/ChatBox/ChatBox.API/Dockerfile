# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

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

COPY ["Directory.Packages.props", "."]
COPY ["Services/ChatBox/ChatBox.API/ChatBox.API.csproj", "Services/ChatBox/ChatBox.API/"]
COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]
COPY ["Services/Auth/Auth.API/Auth.API.csproj", "Services/Auth/Auth.API/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks/BuildingBlocks.Messaging/"]

RUN dotnet restore "./Services/ChatBox/ChatBox.API/ChatBox.API.csproj"

COPY . .
WORKDIR "/src/Services/ChatBox/ChatBox.API"
RUN dotnet build "./ChatBox.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== Publish =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChatBox.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== Final =====
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
COPY Services/ChatBox/ChatBox.API/Lookups/emoease_team_knowledge.md /app/Lookups/

HEALTHCHECK --interval=10s --timeout=3s --retries=6 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ChatBox.API.dll"]
