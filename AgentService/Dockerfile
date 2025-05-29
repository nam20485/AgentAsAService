# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:c149fe7e2be3baccf3cc91e9e6cdcca0ce70f7ca30d5f90796d983ff4f27bd9a AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:b56053d0a8f4627047740941396e76cd9e7a9421c83b1d81b68f10e5019862d7 AS build
WORKDIR /src
COPY ["AgentService/AgentService.csproj", "AgentService/"]
RUN dotnet restore "AgentService/AgentService.csproj"
COPY . .
WORKDIR "/src/AgentService"
RUN dotnet build "AgentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AgentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# The ASPNETCORE_URLS environment variable will be set by Cloud Run during deployment.
EXPOSE 8080 
# Using a default like 8080 for EXPOSE is common if $PORT isn't available at build time for EXPOSE.
# Alternatively, if your build system for Docker allows ARG to ENV expansion for EXPOSE, that could be used.
# However, for Cloud Run, the actual listening port is determined by the PORT env var set at runtime.

ENTRYPOINT ["dotnet", "AgentService.dll"]
