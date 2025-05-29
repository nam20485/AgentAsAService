# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:c149fe7e2be3baccf3cc91e9e6cdcca0ce70f7ca30d5f90796d983ff4f27bd9a AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:b56053d0a8f4627047740941396e76cd9e7a9421c83b1d81b68f10e5019862d7 AS build
WORKDIR /src
COPY ["OrchestratorService/OrchestratorService.csproj", "OrchestratorService/"]
RUN dotnet restore "OrchestratorService/OrchestratorService.csproj"
COPY . .
WORKDIR "/src/OrchestratorService"
RUN dotnet build "OrchestratorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrchestratorService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the port. This is for documentation and can be used by other tools.
# Cloud Run uses the value of the PORT environment variable it sets to route traffic.
# This assumes PORT will be set by the Cloud Run environment.
EXPOSE 8080

ENTRYPOINT ["dotnet", "OrchestratorService.dll"]
