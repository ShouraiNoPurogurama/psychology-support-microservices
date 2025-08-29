FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/AIModeration/AIModeration.API/AIModeration.API.csproj", "Services/AIModeration/AIModeration.API/"]
RUN dotnet restore "Services/AIModeration/AIModeration.API/AIModeration.API.csproj"
COPY . .
WORKDIR "/src/Services/AIModeration/AIModeration.API"
RUN dotnet build "./AIModeration.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AIModeration.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIModeration.API.dll"]
