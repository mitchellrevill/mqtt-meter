# From the repository root
cd mqtt-solution/Clients/MqttTestClient

# Build the client image and start 20 instances (includes RabbitMQ)
docker-compose -f docker-compose.clients.yml up -d --scale meter=20
```

Each container automatically receives a unique `ClientId` based on its hostname, so all 20 meters publish to distinct topics.

### Viewing logs
```powershell
# Follow logs from all meter containers
docker-compose -f docker-compose.clients.yml logs -f meter
```

### Stopping clients
```powershell
docker-compose -f docker-compose.clients.yml down
```

### Running against an existing broker
If RabbitMQ is already running (e.g. via `Infrastructure.Mqtt/docker-compose.yml`), you can skip the embedded broker and just scale the meters:

```powershell
docker-compose -f docker-compose.clients.yml up -d --scale meter=20 --no-deps meter
```
