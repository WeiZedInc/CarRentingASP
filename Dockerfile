# Build stage for .NET
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
# Find the .csproj file in the repository (regardless of name)
COPY ["*.csproj", "./"]
# Run restore for all found .csproj files
RUN dotnet restore

# Copy all source code
COPY . .

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published .NET app
COPY --from=publish /app/publish .

# Create directory for uploads
RUN mkdir -p /app/Uploads
RUN mkdir -p /app/logs

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Determine the dll name dynamically (if needed)
# Otherwise, replace with your actual dll name
ENTRYPOINT ["dotnet", "CarRentingASP.dll"]