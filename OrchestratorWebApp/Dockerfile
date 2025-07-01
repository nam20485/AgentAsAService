# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
RUN apt-get update && apt-get install -y python3
COPY ["OrchestratorWebApp/OrchestratorWebApp.csproj", "OrchestratorWebApp/"]
# Restore the WebAssembly workload
RUN dotnet workload restore "OrchestratorWebApp/OrchestratorWebApp.csproj"
RUN dotnet restore "OrchestratorWebApp/OrchestratorWebApp.csproj"
COPY ["OrchestratorWebApp/", "OrchestratorWebApp/"]
COPY ["SharedLib/", "SharedLib/"]
RUN dotnet build "OrchestratorWebApp/OrchestratorWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrchestratorWebApp/OrchestratorWebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrchestratorWebApp.dll"]
