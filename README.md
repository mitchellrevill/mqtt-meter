# mqtt-meter
MQTT Based Smart Meter

Architecture 
Subscriber -- Broker --- Shared Clients( Logic server x, logic server y)


Logic Server C#
-------------------------------------
DB access layer, extra logic, bill generation etc 


## Getting Started

### Prerequisites
- **Docker Desktop**: Required to run the RabbitMQ MQTT broker in a containerized environment
- **.NET 8.0 SDK**: Required to build and run the DemoWeb.Server API
- **Visual Studio Code**: Running apps, Visual studio code has worked on all our machines but not sure if rider or VisualStudio will be as successful
- **C# + C# DevKit VSC Extensions**: Runnings tests and building program
- **C++ SDKs**: Can be installed from VisualStudio Installer or online
- **PowerShell**: (Optional) For running the automated startup script
- **Flutter SDK**: (Optional) Required for mobile client development

---

### 1. Install Flutter SDK

1. Download the Flutter SDK Windows installer from the [official Flutter website](https://docs.flutter.dev/get-started/install/windows)
2. Run the installer and follow the prompts
3. The installer will:
   - Extract the Flutter SDK
   - Add Flutter to your PATH
   - Run `flutter doctor` to check dependencies

### 2. Run Flutter Doctor

Open a new PowerShell window and run:

```powershell
flutter doctor
```

This command checks your environment and displays a report of Flutter installation status. Address any issues flagged with an **[✗]**.

### 3. Install Visual Studio Code Extensions

1. Open **Visual Studio Code**
2. Go to **Extensions** (Ctrl+Shift+X)
3. Search for "Flutter" and install the extension by Dart Code
4. This automatically installs the Dart extension as well

### 4. Verify Installation

1. Open the **Command Palette** in VS Code (Ctrl+Shift+P)
2. Type `Flutter: New Project` and select **Application**
3. Choose a location and create a test project
4. Press **F5** to run the app and verify everything works

---

## Setting Up RabbitMQ MQTT Broker

The application uses RabbitMQ with MQTT plugin enabled as the message broker. RabbitMQ handles the publish/subscribe messaging pattern between clients and the server.

### Starting RabbitMQ

**Option 1: Using the PowerShell Script (Recommended)**
1. Open PowerShell
2. Navigate to the Infrastructure.Mqtt directory:
   ```powershell
   cd mqtt-solution/Infrastructure.Mqtt/
   ```
3. Run the startup script:
   ```powershell
   .\start-rabbitmq.ps1
   ```
   This script will:
   - Start RabbitMQ in a Docker container
   - Wait for the container to be ready
   - Display connection information

**Option 2: Using Docker Compose Manually**
1. Navigate to the Infrastructure.Mqtt directory:
   ```powershell
   cd mqtt-solution/Infrastructure.Mqtt/
   ```
2. Run Docker Compose:
   ```powershell
   docker-compose up -d
   ```

### Verifying RabbitMQ is Running
- **Management UI**: Open http://localhost:15672 in your browser
  - Username: `admin`
  - Password: `admin`
- **MQTT Port**: 1883 (used by the application)
- **AMQP Port**: 5672 (alternative messaging protocol)

---

## Running the Demo Web Server

The DemoWeb.Server is an ASP.NET Core Web API that provides endpoints for publishing MQTT messages and includes the MQTT Background Service for subscribing to topics.

### Steps to Run

1. **Navigate to the DemoWeb.Server directory**:
   ```powershell
   cd mqtt-solution/DemoWeb.Server/
   ```

2. **Build the project**:
   ```powershell
   dotnet build
   ```

3. **Run the application**:
   ```powershell
   dotnet run
   ```

4. **Access the API**:
   - The server will start on `https://localhost:5001` or `http://localhost:5000`
   - Swagger UI is available at: `https://localhost:5001/swagger`

---

## Using the MQTT API

### Available Endpoints

#### 1. Check MQTT Connection Status
**GET** `/api/Mqtt/status`

Returns the current connection status of the MQTT publisher.

**Example Response**:
```json
{
  "connected": true,
  "timestamp": "2025-10-22T14:30:00Z"
}
```

#### 2. Publish a Meter Reading
**POST** `/api/Mqtt/publish/reading`

Publishes a meter reading to the MQTT broker.

**Request Body**:
```json
{
  "clientId": "meter-001",
  "value": 123.45,
  "unit": "kWh"
}
```

**Response**:
```json
{
  "success": true,
  "topic": "meter/readings/meter-001",
  "message": "Reading published successfully",
  "timestamp": "2025-10-22T14:30:00Z"
}
```

#### 3. Publish Client Status
**POST** `/api/Mqtt/publish/status`

Publishes a status update for a client.

**Request Body**:
```json
{
  "clientId": "meter-001",
  "status": "online",
  "message": "Client connected"
}
```

#### 4. Publish an Alert
**POST** `/api/Mqtt/publish/alert`

Publishes an alert message.

**Request Body**:
```json
{
  "clientId": "meter-001",
  "type": "highUsage",
  "severity": "warning",
  "message": "Energy consumption exceeds threshold",
  "data": {
    "threshold": 100,
    "current": 123.45
  }
}
```

#### 5. Publish Batch Readings
**POST** `/api/Mqtt/publish/readings/batch`

Publishes multiple readings in a single request.

**Request Body**:
```json
{
  "readings": [
    { "clientId": "meter-001", "value": 123.45, "unit": "kWh" },
    { "clientId": "meter-002", "value": 98.76, "unit": "kWh" }
  ]
}
```

---

## MQTT Background Service

### What It Does

The **MqttBackgroundService** is a hosted service that runs automatically when the DemoWeb.Server starts. It performs the following functions:

1. **Automatic Subscription**: On startup, the service connects to the RabbitMQ MQTT broker and subscribes to the `meter/readings/#` topic (wildcarded to receive all meter readings).

2. **Message Processing**: When a reading is published to the broker (via the API or any MQTT client), the background service:
   - Receives the message payload
   - Deserializes the JSON content
   - Logs the reading to a file named `readings.log` in the server directory
   - Logs the event to the application's logging system

3. **Connection Resilience**: The service includes reconnection logic:
   - If the initial connection fails, it logs a warning and continues in "publisher-only" mode
   - Every 30 seconds, it checks the connection status
   - Attempts to reconnect if the connection is lost

4. **Graceful Shutdown**: When the application stops, the service cleanly disconnects from the MQTT broker.

### Message Flow

```
[API Client] 
    |
    | POST /api/Mqtt/publish/reading
    v
[MQTT Publisher] --> [RabbitMQ MQTT Broker] --> [MQTT Subscriber (Background Service)]
                                                      |
                                                      v
                                                [readings.log file]
```

### Viewing Logged Readings

The background service writes all received readings to `readings.log` in the DemoWeb.Server directory. Each entry includes:
- Timestamp
- Topic name
- Full JSON payload

**Example log entry**:
```
2025-10-22 14:30:00 - Topic: meter/readings/meter-001 - Payload: {"ClientId":"meter-001","Timestamp":"2025-10-22T14:30:00Z","Value":123.45,"Unit":"kWh","Metadata":{"source":"api","requestId":"abc123"}}
```

---

## Testing the System

### End-to-End Test

1. **Start RabbitMQ**: Run `.\start-rabbitmq.ps1` in the Infrastructure.Mqtt directory
2. **Start the Server**: Run `dotnet run` in the DemoWeb.Server directory
3. **Publish a Reading**: Send a POST request to `/api/Mqtt/publish/reading`
4. **Verify**: Check the `readings.log` file to see the logged message

### Using Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Expand the `/api/Mqtt/publish/reading` endpoint
3. Click "Try it out"
4. Enter sample data and click "Execute"
5. Check the response and the `readings.log` file

---

## Troubleshooting

### RabbitMQ Container Not Starting
- Ensure Docker Desktop is running
- Check if port 1883, 5672, or 15672 are already in use
- Run `docker logs mqtt-rabbitmq` to view container logs

### Server Fails to Connect to RabbitMQ
- Verify RabbitMQ is running: `docker ps | findstr mqtt-rabbitmq`
- Check the server logs for connection errors
- Ensure the MQTT plugin is enabled (handled by `rabbitmq-plugins.conf`)

### Readings Not Being Logged
- Check if the background service started successfully in the server logs
- Verify the topic name matches the subscription pattern
- Ensure the `readings.log` file has write permissions