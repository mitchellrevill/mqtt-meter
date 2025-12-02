# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first to leverage layer caching during restore
COPY ["mqtt-solution/Clients/MqttTestClient/MqttTestClient.csproj", "Clients/MqttTestClient/"]
COPY ["mqtt-solution/Infrastructure.Mqtt/Infrastructure.Mqtt.csproj", "Infrastructure.Mqtt/"]
COPY ["mqtt-solution/Application/Application.csproj", "Application/"]
COPY ["mqtt-solution/Domain/Domain.csproj", "Domain/"]
COPY ["mqtt-solution/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Clients/MqttTestClient/MqttTestClient.csproj"

# Copy the remaining source code and publish
COPY mqtt-solution/. ./
WORKDIR /src/Clients/MqttTestClient
RUN dotnet publish "MqttTestClient.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MqttTestClient.dll"]
