import 'dart:async';
import 'dart:math';
import 'dart:io' show Platform;
import 'package:fluttertoast/fluttertoast.dart';
import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'mqtt_service.dart';
import '../../config/logger_config.dart';
import '../../domain/entities/grid_alert_model.dart';

/// Service for managing grid alerts and displaying toast notifications
/// Includes a mock alert generator for demonstration purposes
class AlertService {
  Function(GridAlertModel)? _onAlertReceived;
  Function(GridAlertModel)? _internalAlertListener;
  Timer? _mockAlertTimer;
  final Random _random = Random();

  // Callback for showing overlays on desktop platforms
  Function(GridAlertModel)? _showOverlayCallback;

  // Singleton pattern
  static final AlertService _instance = AlertService._internal();
  factory AlertService() => _instance;
  AlertService._internal();

  /// Start listening for grid alerts via MQTT
  /// Also starts a mock alert generator for demonstration
  void startListeningForAlerts({
    Function(GridAlertModel)? onAlertReceived,
    Function(GridAlertModel)? showOverlay,
    bool enableMockAlerts = true,
  }) {
    _onAlertReceived = onAlertReceived;
    _showOverlayCallback = showOverlay;
    LoggerConfig.logAppLifecycle('Starting to listen for grid alerts');

    // Register listener with MQTT service
    _internalAlertListener = (alert) {
      _handleAlert(alert);
      if (_onAlertReceived != null) _onAlertReceived!(alert);
    };
    MqttService().addAlertListener(_internalAlertListener!);

    // Start mock alert generator for demo purposes
    if (enableMockAlerts) {
      _startMockAlertGenerator();
    }
  }

  /// Stop listening for alerts and clean up
  void stopListeningForAlerts() {
    LoggerConfig.logAppLifecycle('Stopping grid alert listener');

    // Stop mock alert generator
    _mockAlertTimer?.cancel();
    _mockAlertTimer = null;

    // Remove MQTT listener
    if (_internalAlertListener != null) {
      MqttService().removeAlertListener(_internalAlertListener!);
      _internalAlertListener = null;
    }

    _onAlertReceived = null;
  }

  /// Handle incoming alert by showing toast notification
  void _handleAlert(GridAlertModel alert) {
    try {
      LoggerConfig.logAppLifecycle(
        'Grid Alert [${alert.severity}]: ${alert.message}',
      );

      // Show toast notification
      _showToast(alert);
    } catch (e) {
      LoggerConfig.logAppLifecycle(
        'Error handling alert: $e',
      );
    }
  }

  /// Display a toast notification for the alert
  void _showToast(GridAlertModel alert) {
    try {
      // Check for desktop platform even though only desktop is supported
      final isDesktop = !kIsWeb &&
          (Platform.isWindows || Platform.isMacOS || Platform.isLinux);

      if (isDesktop && _showOverlayCallback != null) {
        _showOverlayCallback!(alert);
      } else {
        final backgroundColor = _getColorForSeverity(alert.severity);
        final icon = _getIconForSeverity(alert.severity);

        Fluttertoast.showToast(
          msg: '$icon ${alert.message}',
          toastLength: Toast.LENGTH_LONG,
          gravity: ToastGravity.TOP,
          timeInSecForIosWeb: 5,
          backgroundColor: backgroundColor,
          textColor: Colors.white,
          fontSize: 16.0,
        );
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle(
        'Error showing toast notification: $e',
      );
    }
  }

  /// Get background color based on alert severity
  Color _getColorForSeverity(AlertSeverity severity) {
    switch (severity) {
      case AlertSeverity.info:
        return Colors.blue.shade700;
      case AlertSeverity.warning:
        return Colors.orange.shade700;
      case AlertSeverity.critical:
        return Colors.red.shade700;
    }
  }

  /// Get icon emoji based on alert severity
  String _getIconForSeverity(AlertSeverity severity) {
    switch (severity) {
      case AlertSeverity.info:
        return 'ℹ️';
      case AlertSeverity.warning:
        return '⚠️';
      case AlertSeverity.critical:
        return '🚨';
    }
  }

  /// Public method to get color for a given severity
  static Color getColorForSeverity(AlertSeverity severity) {
    switch (severity) {
      case AlertSeverity.info:
        return Colors.blue.shade700;
      case AlertSeverity.warning:
        return Colors.orange.shade700;
      case AlertSeverity.critical:
        return Colors.red.shade700;
    }
  }

  /// method to get icon for a given severity
  static String getIconForSeverity(AlertSeverity severity) {
    switch (severity) {
      case AlertSeverity.info:
        return 'ℹ️';
      case AlertSeverity.warning:
        return '⚠️';
      case AlertSeverity.critical:
        return '🚨';
    }
  }

  /// Generates random alerts at intervals between 30-45 seconds
  void _startMockAlertGenerator() {
    LoggerConfig.logAppLifecycle('Starting mock alert generator');

    void scheduleNextMockAlert() {
      final intervalSeconds = 30 + _random.nextInt(16);

      _mockAlertTimer = Timer(Duration(seconds: intervalSeconds), () {
        _generateMockAlert();
        scheduleNextMockAlert();
      });
    }

    // Generate first alert after 15-30 seconds
    final initialDelaySeconds = 15 + _random.nextInt(16);
    _mockAlertTimer = Timer(Duration(seconds: initialDelaySeconds), () {
      _generateMockAlert();
      scheduleNextMockAlert();
    });
  }

  /// Generate a mock grid alert
  void _generateMockAlert() {
    try {
      final alerts = [
        {
          'message': 'Scheduled maintenance starting in grid sector 4',
          'severity': AlertSeverity.info,
          'affectedArea': 'Sector 4',
        },
        {
          'message': 'High demand detected in your area',
          'severity': AlertSeverity.warning,
          'affectedArea': 'Local Area',
        },
        {
          'message': 'Grid overload detected - reduce consumption',
          'severity': AlertSeverity.critical,
          'affectedArea': 'Multiple Sectors',
        },
        {
          'message': 'Power quality monitoring active',
          'severity': AlertSeverity.info,
          'affectedArea': null,
        },
        {
          'message': 'Voltage fluctuation in distribution network',
          'severity': AlertSeverity.warning,
          'affectedArea': 'Distribution Network',
        },
        {
          'message': 'Emergency load shedding may occur',
          'severity': AlertSeverity.critical,
          'affectedArea': 'All Areas',
        },
        {
          'message': 'Grid capacity at 85%',
          'severity': AlertSeverity.warning,
          'affectedArea': null,
        },
        {
          'message': 'Renewable energy sources offline',
          'severity': AlertSeverity.info,
          'affectedArea': 'Sector 7',
        },
      ];

      final selectedAlert = alerts[_random.nextInt(alerts.length)];

      final mockAlert = GridAlertModel(
        alertId: 'mock-${DateTime.now().millisecondsSinceEpoch}',
        message: selectedAlert['message'] as String,
        severity: selectedAlert['severity'] as AlertSeverity,
        timestamp: DateTime.now(),
        affectedArea: selectedAlert['affectedArea'] as String?,
      );

      LoggerConfig.logAppLifecycle(
          'Generated mock alert: ${mockAlert.message}');

      // Simulate receiving alert via MQTT
      if (_internalAlertListener != null) {
        _internalAlertListener!(mockAlert);
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error generating mock alert: $e');
    }
  }

  /// Check if alert service is active (MQTT connected)
  bool get isActive => MqttService().isConnected;
}
