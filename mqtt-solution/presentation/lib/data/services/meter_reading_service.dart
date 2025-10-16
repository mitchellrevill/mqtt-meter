import 'dart:convert';
import 'dart:async';
import 'package:http/http.dart' as http;
import '../../config/logger_config.dart';

class MeterReadingService {
  static const String _baseUrl = 'http://127.0.0.1:5006';
  static const String _readingsEndpoint = '/api/readings';

  Timer? _readingTimer;
  String? _currentUserId;
  double _currentReading = 0.0;

  // Singleton pattern
  static final MeterReadingService _instance = MeterReadingService._internal();
  factory MeterReadingService() => _instance;
  MeterReadingService._internal();

  // Start sending readings every 30 seconds
  void startSendingReadings(String userId) {
    _currentUserId = userId;
    LoggerConfig.logAppLifecycle('Starting to send readings for user: $userId');

    // Send initial reading
    _sendReading();

    // Set up timer to send readings every 30 seconds
    _readingTimer?.cancel();
    _readingTimer = Timer.periodic(const Duration(seconds: 30), (timer) {
      _sendReading();
    });
  }

  // Stop sending readings
  void stopSendingReadings() {
    LoggerConfig.logAppLifecycle('Stopping meter readings');
    _readingTimer?.cancel();
    _readingTimer = null;
  }

  // Send a single reading to the server
  Future<void> _sendReading() async {
    if (_currentUserId == null) return;

    // Simulate increasing meter reading
    _currentReading += (0.5 + (DateTime.now().millisecond % 100) / 100.0);

    final reading = {
      'userId': _currentUserId,
      'kwhSinceLast': double.parse(_currentReading.toStringAsFixed(2)),
    };

    try {
      LoggerConfig.logAppLifecycle(
        'Sending reading: ${reading['kwhSinceLast']} kWh for user: ${reading['userId']}',
      );

      final response = await http.post(
        Uri.parse('$_baseUrl$_readingsEndpoint'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode(reading),
      );

      if (response.statusCode == 200) {
        LoggerConfig.logAppLifecycle(
          'Reading sent successfully: ${response.body}',
        );
      } else {
        LoggerConfig.logAppLifecycle(
          'Failed to send reading. Status: ${response.statusCode}, Body: ${response.body}',
        );
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error sending reading: $e');
    }
  }

  // Get current reading value
  double get currentReading => _currentReading;

  // Check if readings are being sent
  bool get isActive => _readingTimer?.isActive ?? false;
}
