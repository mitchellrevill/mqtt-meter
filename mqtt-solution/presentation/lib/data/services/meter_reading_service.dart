import 'dart:convert';
import 'dart:async';
import 'package:http/http.dart' as http;
import '../../config/logger_config.dart';

class MeterReadingService {
  static const String _baseUrl = 'http://localhost:5006';
  static const String _readingsEndpoint = '/api/readings';

  Timer? _readingTimer;
  String? _currentUserId;
  double _currentReading = 0.0;

  // Countdown tracking
  int _secondsUntilNextReading = 30;
  Timer? _countdownTimer;
  Function(int)? _onCountdownUpdate;

  // Singleton pattern
  static final MeterReadingService _instance = MeterReadingService._internal();
  factory MeterReadingService() => _instance;
  MeterReadingService._internal();

  // Start sending readings every 30 seconds
  void startSendingReadings(String userId, {Function(int)? onCountdownUpdate}) {
    _currentUserId = userId;
    _onCountdownUpdate = onCountdownUpdate;
    LoggerConfig.logAppLifecycle('Starting to send readings for user: $userId');
    LoggerConfig.logAppLifecycle(
      'Countdown callback registered: ${_onCountdownUpdate != null}',
    );

    // Send initial reading
    _sendReading();

    // Reset countdown and start countdown timer
    _secondsUntilNextReading = 30;
    _startCountdownTimer();

    // Set up timer to send readings every 30 seconds
    _readingTimer?.cancel();
    _readingTimer = Timer.periodic(const Duration(seconds: 30), (timer) {
      _sendReading();
      // Reset countdown when we send a reading
      _secondsUntilNextReading = 30;
      LoggerConfig.logAppLifecycle('Reading sent - countdown reset to 30');
    });
  }

  void _startCountdownTimer() {
    _countdownTimer?.cancel();
    LoggerConfig.logAppLifecycle(
      'Starting countdown timer with callback: ${_onCountdownUpdate != null}',
    );

    // Force immediate callback call to test
    if (_onCountdownUpdate != null) {
      _onCountdownUpdate!(_secondsUntilNextReading);
    }

    _countdownTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_secondsUntilNextReading > 0) {
        _secondsUntilNextReading--;
      } else {
        _secondsUntilNextReading = 30; // Reset when it reaches 0
      }

      // Notify dashboard of countdown update
      if (_onCountdownUpdate != null) {
        try {
          _onCountdownUpdate!(_secondsUntilNextReading);
        } catch (e) {
          LoggerConfig.logAppLifecycle('ERROR calling countdown callback: $e');
        }
      } else {
        LoggerConfig.logAppLifecycle('WARNING: Countdown callback is null!');
      }
    });

    LoggerConfig.logAppLifecycle('Countdown timer created and started');
  }

  // Stop sending readings
  void stopSendingReadings() {
    LoggerConfig.logAppLifecycle('Stopping meter readings');
    _readingTimer?.cancel();
    _countdownTimer?.cancel();
    _readingTimer = null;
    _countdownTimer = null;
    _onCountdownUpdate = null;
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

  // Get current countdown value
  int get secondsUntilNextReading => _secondsUntilNextReading;
}
