# Containerization Guide - Certificate Monitoring & Renewal Platform

This guide provides instructions for containerizing the Certificate Monitoring and Renewal Platform using Docker and Docker Compose. The platform includes a .NET 10 web application, API services, and automated certificate renewal automation.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│           Docker Compose Network                         │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Web App    │  │     API      │  │  Automation  │  │
│  │  .NET 10     │  │   .NET 10    │  │   Service    │  │
│  │  (Port 80)   │  │  (Port 5000) │  │  (Background)│  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│        │                 │                    │          │
│        └─────────────────┼────────────────────┘          │
│                          │                                │
│  ┌──────────────────────┴──────────────────────┐        │
│  │                                              │        │
│  │      PostgreSQL Database                    │        │
│  │      (Certificate & Config Storage)         │        │
│  │                                              │        │
│  └──────────────────────────────────────────────┘        │
│                                                           │
│  ┌──────────────┬──────────────┬──────────────┐         │
│  │   Redis      │  Log Volume  │  Config      │         │
│  │   Cache      │  (ELK/etc)   │  Volume      │         │
│  └──────────────┴──────────────┴──────────────┘         │
│                                                           │
└─────────────────────────────────────────────────────────┘
```

## Prerequisites

### System Requirements
- **Docker**: Version 20.10 or higher
- **Docker Compose**: Version 2.0 or higher
- **Disk Space**: Minimum 10GB for images and volumes
- **Memory**: Minimum 4GB RAM (8GB recommended)

### Installation
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows/Mac)
- [Docker CE](https://docs.docker.com/engine/install/) (Linux)

### Verify Installation
```bash
docker --version
docker compose version
```

## Project Structure

```
AutocertSolution/
├── src/
│   ├── WebApp/
│   │   ├── Dockerfile
│   │   ├── Program.cs
│   │   └── ...
│   ├── API/
│   │   ├── Dockerfile
│   │   ├── Program.cs
│   │   └── ...
│   └── AutomationService/
│       ├── Dockerfile
│       ├── Program.cs
│       └── ...
├── docker-compose.yml
├── docker-compose.prod.yml
├── .dockerignore
└── BUILD_INSTRUCTIONS.md
```

## Building Docker Images

### 1. Create Dockerfile for Web App

**src/WebApp/Dockerfile**
```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10 AS build
WORKDIR /src

# Copy project files
COPY ["WebApp/WebApp.csproj", "WebApp/"]
RUN dotnet restore "WebApp/WebApp.csproj"

COPY . .
RUN dotnet build "WebApp/WebApp.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "WebApp/WebApp.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app

# Install certificates and dependencies
RUN apt-get update && apt-get install -y \
    ca-certificates \
    curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

EXPOSE 80 443
ENTRYPOINT ["dotnet", "WebApp.dll"]
```

### 2. Create Dockerfile for API

**src/API/Dockerfile**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10 AS build
WORKDIR /src

COPY ["API/API.csproj", "API/"]
RUN dotnet restore "API/API.csproj"

COPY . .
RUN dotnet build "API/API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app

RUN apt-get update && apt-get install -y \
    ca-certificates \
    curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/api/health || exit 1

EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "API.dll"]
```

### 3. Create Dockerfile for Automation Service

**src/AutomationService/Dockerfile**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10 AS build
WORKDIR /src

COPY ["AutomationService/AutomationService.csproj", "AutomationService/"]
RUN dotnet restore "AutomationService/AutomationService.csproj"

COPY . .
RUN dotnet build "AutomationService/AutomationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutomationService/AutomationService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10
WORKDIR /app

RUN apt-get update && apt-get install -y \
    ca-certificates \
    openssl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AutomationService.dll"]
```

## Docker Compose Configuration

### Development Environment (docker-compose.yml)

```yaml
version: '3.9'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:16-alpine
    container_name: autocert-postgres
    environment:
      POSTGRES_USER: ${DB_USER:-autocert}
      POSTGRES_PASSWORD: ${DB_PASSWORD:-changeme}
      POSTGRES_DB: ${DB_NAME:-autocert_db}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - autocert-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER:-autocert}"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis Cache
  redis:
    image: redis:7-alpine
    container_name: autocert-redis
    command: redis-server --appendonly yes --requirepass ${REDIS_PASSWORD:-redispass}
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - autocert-network
    healthcheck:
      test: ["CMD", "redis-cli", "--raw", "incr", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Web Application
  webapp:
    build:
      context: .
      dockerfile: src/WebApp/Dockerfile
    container_name: autocert-webapp
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=postgres;Port=5432;Database=${DB_NAME:-autocert_db};User Id=${DB_USER:-autocert};Password=${DB_PASSWORD:-changeme};
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD:-redispass}
      - ApiBaseUrl=http://api:5000
    ports:
      - "80:80"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - autocert-network
    volumes:
      - ./src/WebApp:/app/src
    restart: unless-stopped

  # API Service
  api:
    build:
      context: .
      dockerfile: src/API/Dockerfile
    container_name: autocert-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__DefaultConnection=Server=postgres;Port=5432;Database=${DB_NAME:-autocert_db};User Id=${DB_USER:-autocert};Password=${DB_PASSWORD:-changeme};
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD:-redispass}
    ports:
      - "5000:5000"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - autocert-network
    volumes:
      - ./src/API:/app/src
    restart: unless-stopped

  # Certificate Renewal Automation Service
  automation:
    build:
      context: .
      dockerfile: src/AutomationService/Dockerfile
    container_name: autocert-automation
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=postgres;Port=5432;Database=${DB_NAME:-autocert_db};User Id=${DB_USER:-autocert};Password=${DB_PASSWORD:-changeme};
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD:-redispass}
      - ApiBaseUrl=http://api:5000
      - CertificateCheckInterval=3600
      - RenewalBuffer=30
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - autocert-network
    volumes:
      - ./src/AutomationService:/app/src
      - certificates:/app/certificates
    restart: unless-stopped

networks:
  autocert-network:
    driver: bridge

volumes:
  postgres_data:
  redis_data:
  certificates:
```

### Production Environment (docker-compose.prod.yml)

```yaml
version: '3.9'

services:
  postgres:
    image: postgres:16-alpine
    container_name: autocert-postgres-prod
    environment:
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: ${DB_NAME}
    volumes:
      - postgres_data_prod:/var/lib/postgresql/data
    networks:
      - autocert-prod-network
    restart: always
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: autocert-redis-prod
    command: redis-server --appendonly yes --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis_data_prod:/data
    networks:
      - autocert-prod-network
    restart: always
    healthcheck:
      test: ["CMD", "redis-cli", "--raw", "incr", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  webapp:
    build:
      context: .
      dockerfile: src/WebApp/Dockerfile
    container_name: autocert-webapp-prod
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=postgres;Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD}
      - ApiBaseUrl=http://api:5000
    ports:
      - "80:80"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - autocert-prod-network
    restart: always

  api:
    build:
      context: .
      dockerfile: src/API/Dockerfile
    container_name: autocert-api-prod
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__DefaultConnection=Server=postgres;Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD}
    ports:
      - "5000:5000"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - autocert-prod-network
    restart: always

  automation:
    build:
      context: .
      dockerfile: src/AutomationService/Dockerfile
    container_name: autocert-automation-prod
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=postgres;Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD}
      - ApiBaseUrl=http://api:5000
      - CertificateCheckInterval=3600
      - RenewalBuffer=30
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - autocert-prod-network
    restart: always
    volumes:
      - certificates_prod:/app/certificates

networks:
  autocert-prod-network:
    driver: bridge

volumes:
  postgres_data_prod:
  redis_data_prod:
  certificates_prod:
```

## .dockerignore File

Create `.dockerignore` to exclude unnecessary files from Docker builds:

```
**/.vscode
**/.git
**/.gitignore
**/.dockerignore
**/README.md
**/LICENSE
**/bin
**/obj
**/.vs
**/.DS_Store
**/node_modules
**/*.log
.env
docker-compose*.yml
docs/
tests/
scripts/
```

## Environment Configuration

### .env File (Development)

```env
# Database Configuration
DB_USER=autocert
DB_PASSWORD=changeme
DB_NAME=autocert_db

# Redis Configuration
REDIS_PASSWORD=redispass

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
```

### .env.prod File (Production)

```env
# Database Configuration - Change these values!
DB_USER=${PROD_DB_USER}
DB_PASSWORD=${PROD_DB_PASSWORD}
DB_NAME=${PROD_DB_NAME}

# Redis Configuration - Change this value!
REDIS_PASSWORD=${PROD_REDIS_PASSWORD}

# Application Settings
ASPNETCORE_ENVIRONMENT=Production
```

## Running the Containers

### Build Images

```bash
# Build all images
docker compose build

# Build specific service
docker compose build webapp
```

### Start Services

```bash
# Start all services (development)
docker compose up -d

# Start with logs
docker compose up

# Start production environment
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### View Logs

```bash
# View all logs
docker compose logs -f

# View specific service logs
docker compose logs -f api

# View logs from last 100 lines
docker compose logs --tail=100
```

### Stop Services

```bash
# Stop all services
docker compose down

# Stop and remove volumes
docker compose down -v
```

## Managing the Database

### Run Migrations

```bash
docker compose exec webapp dotnet ef database update
```

### Access PostgreSQL

```bash
docker compose exec postgres psql -U autocert -d autocert_db
```

### Backup Database

```bash
docker compose exec postgres pg_dump -U autocert autocert_db > backup.sql
```

### Restore Database

```bash
cat backup.sql | docker compose exec -T postgres psql -U autocert -d autocert_db
```

## Debugging

### Interactive Terminal

```bash
# Access container shell
docker compose exec webapp bash

# Run dotnet commands
docker compose exec webapp dotnet run --help
```

### View Container Stats

```bash
docker stats
```

### Inspect Network

```bash
docker network inspect autocert-network
```

## Security Best Practices

### 1. Use Environment Variables
- Never hardcode secrets
- Use `.env` files (add to `.gitignore`)
- Rotate credentials regularly

### 2. Image Security
```bash
# Scan for vulnerabilities
docker scan webapp

# Use minimal base images
# Current: mcr.microsoft.com/dotnet/aspnet:10
```

### 3. Network Isolation
- Services communicate via internal network
- Only expose necessary ports
- Use network policies for restrictions

### 4. Volume Permissions
```bash
# Set proper permissions
chmod 600 certificates/
```

## Performance Optimization

### Multi-stage Builds
- Reduces final image size
- Faster deployments
- Smaller attack surface

### Resource Limits

Update `docker-compose.yml`:
```yaml
services:
  webapp:
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
```

## Monitoring & Logging

### Health Checks
All services include health checks configured with:
- 30-second intervals
- 3-second timeout
- 5-retry limit

View health status:
```bash
docker compose ps
```

### Centralized Logging (Optional)

Add ELK Stack:
```yaml
elasticsearch:
  image: docker.elastic.co/elasticsearch/elasticsearch:8.0.0
  environment:
    - discovery.type=single-node
  ports:
    - "9200:9200"

logstash:
  image: docker.elastic.co/logstash/logstash:8.0.0
  volumes:
    - ./logstash.conf:/usr/share/logstash/pipeline/logstash.conf

kibana:
  image: docker.elastic.co/kibana/kibana:8.0.0
  ports:
    - "5601:5601"
```

## Troubleshooting

### Container Won't Start
```bash
# Check logs
docker compose logs <service>

# Inspect image
docker inspect <image-id>
```

### Network Issues
```bash
# Test connectivity
docker compose exec api curl http://postgres:5432

# Check DNS
docker compose exec api nslookup postgres
```

### Port Already in Use
```bash
# Find process using port
lsof -i :80

# Change port in docker-compose.yml
ports:
  - "8080:80"
```

### Database Connection Errors
```bash
# Verify postgres is healthy
docker compose ps

# Check connection string
docker compose exec api printenv | grep ConnectionStrings
```

## Deployment to Azure Container Instances

### Push to Azure Container Registry

```bash
# Login to ACR
az acr login --name <registry-name>

# Tag image
docker tag autocert-webapp <registry-name>.azurecr.io/autocert-webapp:latest

# Push image
docker push <registry-name>.azurecr.io/autocert-webapp:latest
```

### Deploy with Docker Compose

```bash
docker context create aci myaci
docker context use myaci
docker compose up
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Container Images](https://mcr.microsoft.com/product/dotnet/aspnet)
- [Best Practices for Writing Dockerfiles](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [Redis Docker Hub](https://hub.docker.com/_/redis)
