# mqtt-meter
MQTT Based Smart Meter

Architecture 
Subscriber -- Broker --- Shared Clients( Logic server x, logic server y)

Self-hosted MQTT Broker: https://github.com/emqx/emqx


Logic Server C#
-------------------------------------
DB access layer, extra logic, bill generation etc 


Client Server Python/MicroPython 
-------------------------------
Sensor reading, Display logic, MQTT recieve and send logic
(Mock Sensor data)

----------------------------------------

Hardware (all self hosted, preferably in docker images for brokers and logic servers to make multiple of each for maximum scalability)