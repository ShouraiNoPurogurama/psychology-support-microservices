FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Auth/RSAKeyGenerator/RSAKeyGenerator.csproj", "Services/Auth/RSAKeyGenerator/"]
RUN dotnet restore "Services/Auth/RSAKeyGenerator/RSAKeyGenerator.csproj"
COPY . .
WORKDIR "/src/Services/Auth/RSAKeyGenerator"
RUN dotnet build "RSAKeyGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RSAKeyGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RSAKeyGenerator.dll"]
