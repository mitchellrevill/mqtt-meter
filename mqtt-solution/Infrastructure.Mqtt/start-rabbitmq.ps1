# Start RabbitMQ MQTT Broker
# This script launches the RabbitMQ container with MQTT support using Docker Compose

Write-Host "Starting RabbitMQ MQTT broker..." -ForegroundColor Green

# Navigate to the project root (assuming this script is run from there)
# If not, adjust the path as needed
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $projectRoot

# Start Docker Compose (detached mode)
docker-compose up -d

# Wait a moment for startup
Start-Sleep -Seconds 10

# Check if RabbitMQ is running
$containerStatus = docker ps --filter "name=mqtt-rabbitmq" --format "{{.Status}}"
if ($containerStatus -match "Up") {
    Write-Host "RabbitMQ started successfully!" -ForegroundColor Green
    Write-Host "Management UI: http://localhost:15672 (admin/admin)" -ForegroundColor Cyan
    Write-Host "MQTT Port: 1883" -ForegroundColor Cyan
} else {
    Write-Host "Failed to start RabbitMQ. Check Docker and try again." -ForegroundColor Red
}