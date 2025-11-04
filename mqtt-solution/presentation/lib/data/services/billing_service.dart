import 'dart:convert';
import 'dart:async';
import 'package:http/http.dart' as http;
import '../../config/logger_config.dart';
import '../../domain/entities/billing_model.dart';
import '../../domain/entities/api_response.dart';

class BillingService {
  static const String _baseUrl = 'http://localhost:5006';
  static const String _billingEndpoint = '/api/billing';

  Timer? _billingRefreshTimer;
  Function(BillingModel)? _onBillingUpdate;

  // Singleton pattern
  static final BillingService _instance = BillingService._internal();
  factory BillingService() => _instance;
  BillingService._internal();

  // Start fetching billing data periodically
  void startFetchingBilling(
    String userId, {
    Function(BillingModel)? onBillingUpdate,
  }) {
    _onBillingUpdate = onBillingUpdate;
    LoggerConfig.logAppLifecycle('Starting to fetch billing for user: $userId');

    // Fetch initial billing data
    _fetchBilling(userId);

    // Set up timer to fetch billing every 10 seconds
    _billingRefreshTimer?.cancel();
    _billingRefreshTimer = Timer.periodic(const Duration(seconds: 10), (timer) {
      _fetchBilling(userId);
    });
  }

  // Stop fetching billing data
  void stopFetchingBilling() {
    LoggerConfig.logAppLifecycle('Stopping billing fetch');
    _billingRefreshTimer?.cancel();
    _billingRefreshTimer = null;
    _onBillingUpdate = null;
  }

  // Fetch billing data from server using proper model classes
  Future<BillingModel?> _fetchBilling(String userId) async {
    try {
      LoggerConfig.logAppLifecycle('Fetching billing data for user: $userId');

      final response = await http.get(
        Uri.parse('$_baseUrl$_billingEndpoint/$userId'),
        headers: {'Content-Type': 'application/json'},
      );

      if (response.statusCode == 200) {
        final Map<String, dynamic> data = json.decode(response.body);
        final billing = BillingModel.fromJson(data);

        LoggerConfig.logAppLifecycle(
          'Billing data received: \$${billing.totalAmount.toStringAsFixed(2)} for ${billing.totalKwhUsed.toStringAsFixed(2)} kWh',
        );

        // Notify callback of billing update
        if (_onBillingUpdate != null) {
          _onBillingUpdate!(billing);
        }

        return billing;
      } else {
        LoggerConfig.logAppLifecycle(
          'Failed to fetch billing. Status: ${response.statusCode}, Body: ${response.body}',
        );
        return null;
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error fetching billing: $e');
      return null;
    }
  }

  // Reset billing for a user
  Future<bool> resetBilling(String userId) async {
    try {
      LoggerConfig.logAppLifecycle('Resetting billing for user: $userId');

      final response = await http.post(
        Uri.parse('$_baseUrl$_billingEndpoint/$userId/reset'),
        headers: {'Content-Type': 'application/json'},
      );

      if (response.statusCode == 200) {
        final apiResponse = ApiResponse<Map<String, dynamic>>.fromJson(
          json.decode(response.body) as Map<String, dynamic>,
          (data) => data,
        );

        if (apiResponse.success) {
          LoggerConfig.logAppLifecycle(
            'Billing reset successfully: ${apiResponse.message}',
          );
          return true;
        } else {
          LoggerConfig.logAppLifecycle(
            'Server reported error: ${apiResponse.message}',
          );
          return false;
        }
      } else {
        LoggerConfig.logAppLifecycle(
          'Failed to reset billing. Status: ${response.statusCode}',
        );
        return false;
      }
    } catch (e) {
      LoggerConfig.logAppLifecycle('Error resetting billing: $e');
      return false;
    }
  }

  // Get billing data once (for manual refresh)
  Future<BillingModel?> getBilling(String userId) async {
    return await _fetchBilling(userId);
  }

  // Check if billing service is active
  bool get isActive => _billingRefreshTimer?.isActive ?? false;
}
