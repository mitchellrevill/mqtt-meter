import 'dart:async';
import 'mqtt_service.dart';
import '../../config/logger_config.dart';
import '../../domain/entities/billing_model.dart';

class BillingService {
  BillingModel? _lastBilling;
  Function(BillingModel)? _onBillingUpdate;
  Function(BillingModel)? _internalBillingListener;

  // Singleton pattern
  static final BillingService _instance = BillingService._internal();
  factory BillingService() => _instance;
  BillingService._internal();

  // Bridge MQTT billing updates to whatever callback the caller supplies.
  void startFetchingBilling(
    String userId, {
    Function(BillingModel)? onBillingUpdate,
  }) {
    _onBillingUpdate = onBillingUpdate;
    LoggerConfig.logAppLifecycle('Starting to fetch billing for user: $userId');
    // Keep a handle to the listener so it can be removed cleanly later.
    _internalBillingListener = (billing) {
      _lastBilling = billing;
      if (_onBillingUpdate != null) _onBillingUpdate!(billing);
    };
    MqttService().addBillingListener(_internalBillingListener!);
  }

  // Stop relaying billing updates to the UI layer.
  void stopFetchingBilling() {
    LoggerConfig.logAppLifecycle('Stopping billing fetch');
    if (_internalBillingListener != null) {
      MqttService().removeBillingListener(_internalBillingListener!);
      _internalBillingListener = null;
    }
    _onBillingUpdate = null;
  }

  // Issue a billing reset command via MQTT so the backend can zero totals.
  Future<bool> resetBilling(String userId) async {
    try {
      LoggerConfig.logAppLifecycle('Resetting billing for user: $userId');
      if (!MqttService().isConnected) {
        await MqttService().connect(userId);
      }

      await MqttService().publishBillingReset(userId);
      LoggerConfig.logAppLifecycle('Published billing reset command for user $userId');
      return true;
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error resetting billing: $e');
      return false;
    }
  }

  // Return the most recent billing snapshot we saw over MQTT.
  Future<BillingModel?> getBilling(String userId) async {
    return _lastBilling;
  }

  // Check if billing service is active (MQTT connected)
  bool get isActive => MqttService().isConnected;
}
