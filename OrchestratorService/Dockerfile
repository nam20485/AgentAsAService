# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
COPY ["OrchestratorService/OrchestratorService.csproj", "OrchestratorService/"]
COPY ["SharedLib/SharedLib.csproj", "SharedLib/"]
RUN dotnet restore "OrchestratorService/OrchestratorService.csproj"
COPY ["OrchestratorService/", "OrchestratorService/"]
COPY ["SharedLib/", "SharedLib/"]
RUN dotnet build "OrchestratorService/OrchestratorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrchestratorService/OrchestratorService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the port. This is for documentation and can be used by other tools.
# Cloud Run uses the value of the PORT environment variable it sets to route traffic.
# This assumes PORT will be set by the Cloud Run environment.
EXPOSE 8080

ENTRYPOINT ["dotnet", "OrchestratorService.dll"]
