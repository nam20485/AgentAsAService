# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AgentService/AgentService.csproj", "AgentService/"]
COPY ["SharedLib/SharedLib.csproj", "SharedLib/"]
RUN dotnet restore "AgentService/AgentService.csproj"
COPY . .
WORKDIR "/src/AgentService"
RUN dotnet build "AgentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AgentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# The ASPNETCORE_URLS environment variable will be set by Cloud Run during deployment.
EXPOSE 8080 
# Using a default like 8080 for EXPOSE is common if $PORT isn't available at build time for EXPOSE.
# Alternatively, if your build system for Docker allows ARG to ENV expansion for EXPOSE, that could be used.
# However, for Cloud Run, the actual listening port is determined by the PORT env var set at runtime.

ENTRYPOINT ["dotnet", "AgentService.dll"]
