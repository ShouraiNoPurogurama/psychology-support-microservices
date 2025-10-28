# ===== Base (runtime) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update \
  && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/* \
  && mkdir -p /app/Assets /https \
  && chmod 755 /app /app/Assets /https \
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
COPY ["Services/Media/Media.API/Media.API.csproj", "Services/Media/Media.API/"]
COPY ["Services/Media/Media.Application/Media.Application.csproj", "Services/Media/Media.Application/"]
COPY ["Services/Media/Media.Domain/Media.Domain.csproj", "Services/Media/Media.Domain/"]
COPY ["Services/Media/Media.Infrastructure/Media.Infrastructure.csproj", "Services/Media/Media.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]

RUN dotnet restore "Services/Media/Media.API/Media.API.csproj"

COPY . .
WORKDIR "/src/Services/Media/Media.API"
RUN dotnet build "./Media.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== Publish =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Media.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== Final =====
FROM base AS final
WORKDIR /app

# app binaries
COPY --from=publish /app/publish .

# lookup asset — copy thẳng vào /app/Assets
# (đường nguồn là đường trong repo của cậu)
COPY Services/Media/Media.API/Assets/emo_base.jpg /app/Assets/

# Nếu từng gặp lỗi quyền, có thể thêm (cần root):
# USER root
# RUN chown $APP_UID:$APP_GID /app/Assets/emo_base.jpg && chm_
