# Use official Microsoft .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set working directory inside the container
WORKDIR /src

# Copy everything from the build context (PsychologySupport/) into /src in container
COPY . .

# Build and publish the LifeStyles.API project
RUN dotnet publish \
    PsychologySupport/Services/LifeStyles/LifeStyles.API/LifeStyles.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# Use ASP.NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Copy published output from the build stage
COPY --from=build /app/publish .

# Expose ports your app listens on (adjust if needed)
EXPOSE 8080
EXPOSE 8081

# Set the entrypoint to run the app
ENTRYPOINT ["dotnet", "LifeStyles.API.dll"]
