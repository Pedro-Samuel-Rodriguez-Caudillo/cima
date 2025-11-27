# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file
COPY ["cima.sln", "./"]

# Copy all project files for restore
COPY ["src/cima.Blazor/cima.Blazor.csproj", "src/cima.Blazor/"]
COPY ["src/cima.Blazor.Client/cima.Blazor.Client.csproj", "src/cima.Blazor.Client/"]
COPY ["src/cima.Application/cima.Application.csproj", "src/cima.Application/"]
COPY ["src/cima.Application.Contracts/cima.Application.Contracts.csproj", "src/cima.Application.Contracts/"]
COPY ["src/cima.Domain/cima.Domain.csproj", "src/cima.Domain/"]
COPY ["src/cima.Domain.Shared/cima.Domain.Shared.csproj", "src/cima.Domain.Shared/"]
COPY ["src/cima.EntityFrameworkCore/cima.EntityFrameworkCore.csproj", "src/cima.EntityFrameworkCore/"]
COPY ["src/cima.HttpApi/cima.HttpApi.csproj", "src/cima.HttpApi/"]
COPY ["src/cima.HttpApi.Client/cima.HttpApi.Client.csproj", "src/cima.HttpApi.Client/"]

# Restore dependencies
RUN dotnet restore "src/cima.Blazor/cima.Blazor.csproj"

# Copy all source code
COPY . .

# Install Node.js for Tailwind CSS build
WORKDIR "/src/src/cima.Blazor.Client"
RUN apt-get update && apt-get install -y nodejs npm
RUN npm install
RUN npm run build:css

# Build application
WORKDIR "/src/src/cima.Blazor"
RUN dotnet build "cima.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "/src/src/cima.Blazor/cima.Blazor.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for uploaded images
RUN mkdir -p /app/wwwroot/images/listings

# Set environment variables for STAGING
ENV ASPNETCORE_ENVIRONMENT=Staging
ENV ASPNETCORE_URLS=http://+:8080
ENV Kestrel__Endpoints__Http__Url=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "cima.Blazor.dll"]
