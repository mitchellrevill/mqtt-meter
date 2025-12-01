import 'dart:async';
import 'dart:convert';
import 'package:mqtt_client/mqtt_client.dart';
import 'package:mqtt_client/mqtt_server_client.dart';
import '../../config/logger_config.dart';
import '../../domain/entities/billing_model.dart';
import '../../domain/entities/grid_alert_model.dart';

const String _defaultMqttHost =
    String.fromEnvironment('MQTT_HOST', defaultValue: 'localhost');
const int _defaultMqttPort =
    int.fromEnvironment('MQTT_PORT', defaultValue: 1883);
const String _defaultMqttUsername =
    String.fromEnvironment('MQTT_USERNAME', defaultValue: 'admin');
const String _defaultMqttPassword =
    String.fromEnvironment('MQTT_PASSWORD', defaultValue: 'admin');

///  MQTT client service for the presentation app.
/// - Connects to an MQTT broker (default: localhost:1883)
/// - Subscribes to meter readings and billing topics for the given userId
/// - Publishes readings when asked
class MqttService {
  static final MqttService _instance = MqttService._internal();
  factory MqttService() => _instance;
  MqttService._internal();

  MqttServerClient? _client;
  String _clientId = 'flutter-client';
  bool _connected = false;

  // We support multiple listeners so different parts of the app can independently
  // react to incoming MQTT messages and connection changes.
  final List<Function(Map<String, dynamic>)> _meterReadingListeners = [];
  final List<Function(BillingModel)> _billingListeners = [];
  final List<Function(bool)> _connectionListeners = [];
  final List<Function(GridAlertModel)> _alertListeners = [];

  /// Register a listener for meter reading messages (payload -> Map)
  void addMeterReadingListener(Function(Map<String, dynamic>) listener) {
    _meterReadingListeners.add(listener);
  }

  /// Unregister previously registered meter reading listener
  void removeMeterReadingListener(Function(Map<String, dynamic>) listener) {
    _meterReadingListeners.remove(listener);
  }

  /// Register a listener for billing updates
  void addBillingListener(Function(BillingModel) listener) {
    _billingListeners.add(listener);
  }

  /// Unregister a billing listener
  void removeBillingListener(Function(BillingModel) listener) {
    _billingListeners.remove(listener);
  }

  /// Register a listener for grid alerts
  void addAlertListener(Function(GridAlertModel) listener) {
    _alertListeners.add(listener);
  }

  /// Unregister an alert listener
  void removeAlertListener(Function(GridAlertModel) listener) {
    _alertListeners.remove(listener);
  }

  /// Register a connection state listener
  void addConnectionListener(Function(bool) listener) {
    _connectionListeners.add(listener);
  }

  /// Unregister a connection listener
  void removeConnectionListener(Function(bool) listener) {
    _connectionListeners.remove(listener);
  }

  /// Connect to an MQTT broker. Defaults to localhost:1883
  Future<bool> connect(
    String userId, {
    String host = _defaultMqttHost,
    int port = _defaultMqttPort,
    String? username = _defaultMqttUsername,
    String? password = _defaultMqttPassword,
  }) async {
    try {
      if (_connected) return true;

      final effectiveUsername =
          (username != null && username.isNotEmpty) ? username : null;
      final effectivePassword =
          (password != null && password.isNotEmpty) ? password : null;
      final maskedUser = effectiveUsername ?? 'anonymous';
      LoggerConfig.logAppLifecycle(
          'MqttService: connecting to $host:$port as $maskedUser');

      _clientId = 'flutter-${DateTime.now().millisecondsSinceEpoch}';

      final client = MqttServerClient(host, _clientId);
      client.port = port;
      client.logging(on: false);
      client.keepAlivePeriod = 20;
      client.onConnected = _onConnected;
      client.onDisconnected = _onDisconnected;
      client.onSubscribed =
          (topic) => LoggerConfig.logAppLifecycle('Subscribed to $topic');

      // Use a simple connection message
      final connMess = MqttConnectMessage()
          .withClientIdentifier(_clientId)
          .startClean()
          .withWillQos(MqttQos.atLeastOnce);

      if (effectiveUsername != null && effectivePassword != null) {
        connMess.authenticateAs(effectiveUsername, effectivePassword);
      } else {
        LoggerConfig.logAppLifecycle(
            'MqttService: no credentials supplied, broker may reject connection');
      }

      client.connectionMessage = connMess;

      _client = client;

      await client.connect();

      if (client.connectionStatus?.state == MqttConnectionState.connected) {
        _connected = true;
        for (final l in _connectionListeners) {
          try {
            l(true);
          } catch (e) {
            LoggerConfig.logAppLifecycle(
                'MqttService: connection listener error: $e');
          }
        }

        // Subscribe to the user topics for readings, billing, and alerts
        final readingTopic = 'meters/readings/$userId';
        final billingTopic = 'meters/billing/$userId';
        final alertTopic = 'alerts/grid';

        client.subscribe(readingTopic, MqttQos.atLeastOnce);
        client.subscribe(billingTopic, MqttQos.atLeastOnce);
        client.subscribe(alertTopic, MqttQos.atLeastOnce);

        // Listen for messages
        client.updates?.listen(_onMessage);

        LoggerConfig.logAppLifecycle('MqttService: connected and subscribed');
        return true;
      }

      LoggerConfig.logAppLifecycle(
          'MqttService: failed to connect, status: ${client.connectionStatus}');
      _connected = false;
      for (final l in _connectionListeners) {
        try {
          l(false);
        } catch (e) {
          LoggerConfig.logAppLifecycle(
              'MqttService: connection listener error: $e');
        }
      }
      return false;
    } catch (e) {
      LoggerConfig.logAppLifecycle('MqttService: error connecting: $e');
      _connected = false;
      for (final l in _connectionListeners) {
        try {
          l(false);
        } catch (e) {
          LoggerConfig.logAppLifecycle(
              'MqttService: connection listener error: $e');
        }
      }
      return false;
    }
  }

  Future<void> disconnect() async {
    try {
      if (_client == null) return;
      _client?.disconnect();
      _connected = false;
      for (final l in _connectionListeners) {
        try {
          l(false);
        } catch (e) {
          LoggerConfig.logAppLifecycle(
              'MqttService: connection listener error: $e');
        }
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('MqttService: error disconnecting: $e');
    }
  }

  bool get isConnected => _connected;

  Future<void> publishReading(String userId, double value) async {
    if (!isConnected || _client == null) {
      LoggerConfig.logAppLifecycle(
          'MqttService: not connected — cannot publish reading');
      return;
    }

    final topic = 'meters/readings/$userId';
    final payload = json.encode({
      'userId': userId,
      'value': value,
      'timestamp': DateTime.now().toIso8601String()
    });

    final builder = MqttClientPayloadBuilder();
    builder.addString(payload);

    try {
      _client!.publishMessage(topic, MqttQos.atLeastOnce, builder.payload!);
      LoggerConfig.logAppLifecycle('MqttService: published reading to $topic');
    } catch (e) {
      LoggerConfig.logAppLifecycle('MqttService: error publishing reading: $e');
    }
  }

  /// Publish a billing reset command for the given userId
  Future<void> publishBillingReset(String userId) async {
    if (!isConnected || _client == null) {
      LoggerConfig.logAppLifecycle(
          'MqttService: not connected — cannot publish billing reset');
      return;
    }

    final topic = 'meters/billing/$userId/reset';
    final payload = json.encode({
      'userId': userId,
      'command': 'reset',
      'timestamp': DateTime.now().toIso8601String()
    });

    final builder = MqttClientPayloadBuilder();
    builder.addString(payload);

    try {
      _client!.publishMessage(topic, MqttQos.atLeastOnce, builder.payload!);
      LoggerConfig.logAppLifecycle(
          'MqttService: published billing reset to $topic');
    } catch (e) {
      LoggerConfig.logAppLifecycle(
          'MqttService: error publishing billing reset: $e');
    }
  }

  void _onMessage(List<MqttReceivedMessage<MqttMessage?>>? events) {
    try {
      if (events == null || events.isEmpty) return;

      for (final event in events) {
        final recMess = event.payload as MqttPublishMessage;
        final payload =
            MqttPublishPayload.bytesToStringAsString(recMess.payload.message);
        final topic = event.topic;

        LoggerConfig.logAppLifecycle(
            'MqttService: message on topic $topic -> $payload');

        try {
          final data = json.decode(payload) as Map<String, dynamic>;

          if (topic.startsWith('meters/readings')) {
            for (final l in _meterReadingListeners) {
              try {
                l(data);
              } catch (e) {
                LoggerConfig.logAppLifecycle(
                    'MqttService: meter listener error: $e');
              }
            }
          } else if (topic.startsWith('meters/billing')) {
            final billing = BillingModel.fromJson(data);
            for (final l in _billingListeners) {
              try {
                l(billing);
              } catch (e) {
                LoggerConfig.logAppLifecycle(
                    'MqttService: billing listener error: $e');
              }
            }
          } else if (topic.startsWith('alerts/grid')) {
            final alert = GridAlertModel.fromJson(data);
            for (final l in _alertListeners) {
              try {
                l(alert);
              } catch (e) {
                LoggerConfig.logAppLifecycle(
                    'MqttService: alert listener error: $e');
              }
            }
          }
        } catch (e) {
          LoggerConfig.logAppLifecycle(
              'MqttService: error parsing message payload: $e');
        }
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('MqttService: error handling messages: $e');
    }
  }

  void _onConnected() {
    LoggerConfig.logAppLifecycle('MqttService: onConnected');
    _connected = true;
    for (final l in _connectionListeners) {
      try {
        l(true);
      } catch (e) {
        LoggerConfig.logAppLifecycle(
            'MqttService: connection listener error: $e');
      }
    }
  }

  void _onDisconnected() {
    LoggerConfig.logAppLifecycle('MqttService: onDisconnected');
    _connected = false;
    for (final l in _connectionListeners) {
      try {
        l(false);
      } catch (e) {
        LoggerConfig.logAppLifecycle(
            'MqttService: connection listener error: $e');
      }
    }
  }
}
