# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["KubernetesApp.csproj", "./"]
RUN dotnet restore "KubernetesApp.csproj"

# Copy source code
COPY . .
RUN dotnet build "KubernetesApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "KubernetesApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' --shell /bin/bash --uid 1001 dotnetuser
USER dotnetuser

# Copy published application
COPY --from=publish /app/publish .

# Health check for Docker
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "KubernetesApp.dll"]