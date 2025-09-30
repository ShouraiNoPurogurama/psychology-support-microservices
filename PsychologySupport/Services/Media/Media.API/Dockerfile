FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Media/Media.API/Media.API.csproj", "Services/Media/Media.API/"]
COPY ["Services/Media/Media.Application/Media.Application.csproj", "Services/Media/Media.Application/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]
COPY ["Services/Media/Media.Domain/Media.Domain.csproj", "Services/Media/Media.Domain/"]
COPY ["Services/Media/Media.Infrastructure/Media.Infrastructure.csproj", "Services/Media/Media.Infrastructure/"]
RUN dotnet restore "Services/Media/Media.API/Media.API.csproj"
COPY . .
WORKDIR "/src/Services/Media/Media.API"
RUN dotnet build "./Media.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Media.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Media.API.dll"]
