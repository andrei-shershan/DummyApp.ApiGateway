# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# Install mkcert root CA so the container trusts *.dummy.localhost certificates
COPY certs/rootCA.pem /usr/local/share/ca-certificates/mkcert-ca.crt
RUN update-ca-certificates
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage optimized for local development
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["src/DummyApp.ApiGateway.WebApi/DummyApp.ApiGateway.WebApi.csproj", "src/DummyApp.ApiGateway.WebApi/"]
COPY ["src/DummyApp.ApiGateway.Infrastructure/DummyApp.ApiGateway.Infrastructure.csproj", "src/DummyApp.ApiGateway.Infrastructure/"]
RUN dotnet restore "./src/DummyApp.ApiGateway.WebApi/DummyApp.ApiGateway.WebApi.csproj"
COPY ["src/DummyApp.ApiGateway.WebApi/", "src/DummyApp.ApiGateway.WebApi/"]
COPY ["src/DummyApp.ApiGateway.Infrastructure/", "src/DummyApp.ApiGateway.Infrastructure/"]
WORKDIR "/src/src/DummyApp.ApiGateway.WebApi"
RUN dotnet build "./DummyApp.ApiGateway.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./DummyApp.ApiGateway.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DummyApp.ApiGateway.WebApi.dll"]