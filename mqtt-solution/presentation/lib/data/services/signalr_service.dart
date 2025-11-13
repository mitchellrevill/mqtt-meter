import 'dart:async';
import 'package:signalr_netcore/signalr_client.dart';
import '../../config/logger_config.dart';
import '../../domain/entities/billing_model.dart';

/// Service to manage SignalR connection for real-time updates
class SignalRService {
  static const String _baseUrl = 'http://localhost:5006';
  static const String _hubPath = '/hub/billing';

  HubConnection? _hubConnection;
  bool _isConnected = false;

  // Callbacks for real-time updates
  Function(Map<String, dynamic>)? onMeterReading;
  Function(BillingModel)? onBillingUpdate;
  Function(bool)? onConnectionChanged;

  // Singleton pattern
  static final SignalRService _instance = SignalRService._internal();
  factory SignalRService() => _instance;
  SignalRService._internal();

  bool get isConnected => _isConnected;

  /// Connect to SignalR hub and register for a specific user
  Future<bool> connect(String userId) async {
    try {
      if (_isConnected) {
        LoggerConfig.logAppLifecycle('Already connected to SignalR');
        return true;
      }

      LoggerConfig.logAppLifecycle('Connecting to SignalR at $_baseUrl$_hubPath');

      // Build the hub connection
      _hubConnection = HubConnectionBuilder()
          .withUrl('$_baseUrl$_hubPath')
          .withAutomaticReconnect()
          .build();

      // Set up event handlers
      _hubConnection!.onclose(({error}) {
        LoggerConfig.logAppLifecycle('SignalR connection closed: ${error ?? "No error"}');
        _isConnected = false;
        onConnectionChanged?.call(false);
      });

      _hubConnection!.onreconnecting(({error}) {
        LoggerConfig.logAppLifecycle('SignalR reconnecting: ${error ?? "No error"}');
        _isConnected = false;
        onConnectionChanged?.call(false);
      });

      _hubConnection!.onreconnected(({connectionId}) {
        LoggerConfig.logAppLifecycle('SignalR reconnected with ID: ${connectionId ?? "unknown"}');
        _isConnected = true;
        onConnectionChanged?.call(true);
        // Re-register after reconnection
        _register(userId);
      });

      // Set up message handlers
      _hubConnection!.on('MeterReading', _handleMeterReading);
      _hubConnection!.on('BillingUpdate', _handleBillingUpdate);
      _hubConnection!.on('RegistrationConfirmed', _handleRegistrationConfirmed);

      // Start the connection
      await _hubConnection!.start();
      _isConnected = true;
      onConnectionChanged?.call(true);

      LoggerConfig.logAppLifecycle('SignalR connected successfully');

      // Register this connection with the user ID
      await _register(userId);

      return true;
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error connecting to SignalR: $e');
      _isConnected = false;
      onConnectionChanged?.call(false);
      return false;
    }
  }

  /// Register the connection with a user ID
  Future<void> _register(String userId) async {
    try {
      if (_hubConnection != null && _isConnected) {
        await _hubConnection!.invoke('Register', args: [userId]);
        LoggerConfig.logAppLifecycle('Registered with SignalR as user: $userId');
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error registering with SignalR: $e');
    }
  }

  /// Handle meter reading updates from server
  void _handleMeterReading(List<Object?>? arguments) {
    try {
      if (arguments != null && arguments.isNotEmpty) {
        final data = arguments[0] as Map<String, dynamic>;
        LoggerConfig.logAppLifecycle('Received meter reading: $data');
        onMeterReading?.call(data);
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error handling meter reading: $e');
    }
  }

  /// Handle billing updates from server
  void _handleBillingUpdate(List<Object?>? arguments) {
    try {
      if (arguments != null && arguments.isNotEmpty) {
        final data = arguments[0] as Map<String, dynamic>;
        final billing = BillingModel.fromJson(data);
        LoggerConfig.logAppLifecycle('Received billing update: \$${billing.totalAmount.toStringAsFixed(2)}');
        onBillingUpdate?.call(billing);
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error handling billing update: $e');
    }
  }

  /// Handle registration confirmation
  void _handleRegistrationConfirmed(List<Object?>? arguments) {
    try {
      if (arguments != null && arguments.isNotEmpty) {
        final userId = arguments[0] as String;
        LoggerConfig.logAppLifecycle('Registration confirmed for user: $userId');
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error handling registration confirmation: $e');
    }
  }

  /// Disconnect from SignalR hub
  Future<void> disconnect() async {
    try {
      if (_hubConnection != null && _isConnected) {
        await _hubConnection!.stop();
        _isConnected = false;
        onConnectionChanged?.call(false);
        LoggerConfig.logAppLifecycle('Disconnected from SignalR');
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error disconnecting from SignalR: $e');
    }
  }

  /// Clean up resources
  void dispose() {
    disconnect();
    _hubConnection = null;
    onMeterReading = null;
    onBillingUpdate = null;
    onConnectionChanged = null;
  }
}
