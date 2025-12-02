# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first to leverage layer caching during restore
COPY ["mqtt-solution/Server/Server.csproj", "Server/"]
COPY ["mqtt-solution/Application/Application.csproj", "Application/"]
COPY ["mqtt-solution/Domain/Domain.csproj", "Domain/"]
COPY ["mqtt-solution/Infrastructure.Mqtt/Infrastructure.Mqtt.csproj", "Infrastructure.Mqtt/"]
COPY ["mqtt-solution/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Server/Server.csproj"

# Copy the remaining source code and publish
COPY mqtt-solution/. ./
WORKDIR /src/Server
RUN dotnet publish "Server.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8443
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Server.dll"]
