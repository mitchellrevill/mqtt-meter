import 'dart:async';
import 'dart:math';
import 'mqtt_service.dart';
import '../../config/logger_config.dart';
import '../../domain/entities/create_reading_request.dart';

class MeterReadingService {
  static const int _minIntervalSeconds = 15;
  static const int _maxIntervalSeconds = 45;
  final Random _rng = Random();
  
  Timer? _readingTimer;
  String? _currentUserId;
  double _currentReading = 0.0;

  // Countdown tracking
  int _secondsUntilNextReading = _maxIntervalSeconds;
  Timer? _countdownTimer;
  Function(int)? _onCountdownUpdate;

  // Singleton pattern
  static final MeterReadingService _instance = MeterReadingService._internal();
  factory MeterReadingService() => _instance;
  MeterReadingService._internal();

   // Start sending readings between min and max intervals
  void startSendingReadings(String userId, {Function(int)? onCountdownUpdate}) {
    _currentUserId = userId;
    _onCountdownUpdate = onCountdownUpdate;
    LoggerConfig.logAppLifecycle('Starting to send readings for user: $userId');
    LoggerConfig.logAppLifecycle(
      'Countdown callback registered: ${_onCountdownUpdate != null}',
    );

    // Send initial reading
    _sendReading();

    _scheduleNextReading();
  }

  void _scheduleNextReading() {
    _readingTimer?.cancel();

    final int nextDelay = _rng.nextInt(
          _maxIntervalSeconds - _minIntervalSeconds + 1,
        ) +
        _minIntervalSeconds;

    _secondsUntilNextReading = nextDelay;
    _startCountdownTimer();

    _readingTimer = Timer(Duration(seconds: nextDelay), () {
      _sendReading();
      _scheduleNextReading();
    });

    LoggerConfig.logAppLifecycle(
      'Next reading scheduled in $nextDelay seconds',
    );
  }

  void _startCountdownTimer() {
    _countdownTimer?.cancel();
    LoggerConfig.logAppLifecycle(
      'Starting countdown timer with callback: ${_onCountdownUpdate != null}',
    );

    // Immediately push the current countdown so the UI shows a value before the first tick.
    if (_onCountdownUpdate != null) {
      _onCountdownUpdate!(_secondsUntilNextReading);
    }

    _countdownTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_secondsUntilNextReading > 0) {
        _secondsUntilNextReading--;
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

      if (_secondsUntilNextReading <= 0) {
        LoggerConfig.logAppLifecycle(
          'Countdown reached zero - awaiting next interval',
        );
        timer.cancel();
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

  // Send a single reading to the server using proper model classes
  Future<void> _sendReading() async {
    if (_currentUserId == null) return;

    // Simulate increasing meter reading
    _currentReading += (0.5 + (DateTime.now().millisecond % 100) / 100.0);

    // Build a typed payload before handing it to the MQTT service
    final readingRequest = CreateReadingRequest(
      userId: _currentUserId!,
      value: double.parse(_currentReading.toStringAsFixed(2)),
    );

    try {
      LoggerConfig.logAppLifecycle(
        'Sending reading: ${readingRequest.value} kWh for user: ${readingRequest.userId}',
      );

      // Ensure connected and publish via MQTT
      if (!MqttService().isConnected) {
        // try to connect using current user id before publishing
        await MqttService().connect(readingRequest.userId);
      }

      await MqttService().publishReading(readingRequest.userId, readingRequest.value);
      
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
