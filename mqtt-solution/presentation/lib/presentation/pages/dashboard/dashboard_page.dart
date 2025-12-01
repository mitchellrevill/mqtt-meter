import 'package:flutter/material.dart';
import '../../widgets/dashboard/connection_status_widget.dart';
import '../../widgets/dashboard/total_bill_widget.dart';
import '../../widgets/dashboard/next_reading_countdown_widget.dart';
import '../../widgets/dashboard/bill_updates_list_widget.dart';
import '../../../data/services/meter_reading_service.dart';
import '../../../data/services/billing_service.dart';
import '../../../data/services/mqtt_service.dart';
import '../../../data/services/alert_service.dart';
import '../../../domain/entities/billing_model.dart';
import '../../../core/utils/logger_mixin.dart';

class DashboardPage extends StatefulWidget {
  const DashboardPage({super.key});

  @override
  State<DashboardPage> createState() => _DashboardPageState();
}

class _DashboardPageState extends State<DashboardPage> with LoggerMixin {
  // Data variables - will be populated from API/MQTT later
  bool isConnected = false; // Default to disconnected until proven otherwise
  double? totalBill; // null until data is available
  Duration? timeUntilNextReading; // null until data is available
  List<BillUpdate> billUpdates = []; // empty list by default

  // Billing information using proper model class
  BillingModel? currentBilling;
  double totalKwhUsed = 0.0;
  double ratePerKwh = 0.15;
  double? _previousBillingTotalKwh;
  double? _previousBillingTotalAmount;
  DateTime? _previousBillingTimestamp;
  int? _previousReadingCount;

  // Services
  final MeterReadingService _meterService = MeterReadingService();
  // SignalR removed - using MQTT only
  final BillingService _billingService = BillingService();
  final _mqttService = MqttService();
  final AlertService _alertService = AlertService();

  // Keep references to listeners so we can remove them on dispose
  Function(bool)? _connectionListener;
  Function(Map<String, dynamic>)? _meterListener;
  Function(BillingModel)? _billingListener;

  // Automatic user ID - no user input needed
  static const String _automaticUserId = 'user-001';

  @override
  void initState() {
    super.initState();

    logInfo('Dashboard initializing...');
    logDebug(
      'Connection status: ${isConnected ? "Connected" : "Disconnected"}',
    );

    if (totalBill != null) {
      logInfo('Total bill loaded: \$${totalBill!.toStringAsFixed(2)}');
    } else {
      logInfo('No total bill data available');
    }

    logDebug('Loading ${billUpdates.length} bill updates');

    // Log each bill update for debugging
    for (int i = 0; i < billUpdates.length; i++) {
      logTrace(
        'Bill update $i: ${billUpdates[i].energyUsage} kWh - \$${billUpdates[i].amount.toStringAsFixed(2)}',
      );
    }

    // Automatically start meter readings and MQTT when the app launches
    _startAutomaticMeterReadings();
    _startFetchingBilling();
    _startAlertService();

    logInfo('Dashboard initialization complete');
  }

  @override
  void dispose() {
    _meterService.stopSendingReadings();

    // Stop BillingService first (it may remove its own listener)
    _billingService.stopFetchingBilling();

    // Stop AlertService
    _alertService.stopListeningForAlerts();

    // Remove listeners registered by this page
    if (_connectionListener != null) {
      _mqttService.removeConnectionListener(_connectionListener!);
    }
    if (_meterListener != null) {
      _mqttService.removeMeterReadingListener(_meterListener!);
    }
    if (_billingListener != null) {
      _mqttService.removeBillingListener(_billingListener!);
    }

    // Disconnect from MQTT broker
    _mqttService.disconnect();

    super.dispose();
  }

  void _startAutomaticMeterReadings() {
    setState(() {
      isConnected = true;
    });

    // Start meter readings with countdown callback
    _meterService.startSendingReadings(
      _automaticUserId,
      onCountdownUpdate: (seconds) {
        setState(() {
          timeUntilNextReading = Duration(seconds: seconds);
        });
      },
    );
    // Connect to MQTT broker so the app can publish/subscribe directly
    _connectToMqtt();

    logInfo('Automatically started meter readings for user: $_automaticUserId');
  }

  // SignalR removed — no longer supported in presentation app

  void _connectToMqtt() async {
    _connectionListener = (connected) {
      setState(() {
        isConnected = connected;
      });
      logInfo('MQTT connection changed: $connected');
    };
    _mqttService.addConnectionListener(_connectionListener!);

    _meterListener = (reading) {
      logInfo('Received meter reading via MQTT: $reading');
      // Keep the same behaviour as SignalR for meter reading events
      if (reading.containsKey('value') && reading.containsKey('timestamp')) {
        final value = (reading['value'] as num).toDouble();
        final amount = value * ratePerKwh;

        setState(() {
          totalKwhUsed += value;
          totalBill = (totalBill ?? 0.0) + amount;
        });
      }
    };
    _mqttService.addMeterReadingListener(_meterListener!);

    _billingListener = (billing) {
      setState(() {
        currentBilling = billing;
        totalBill = billing.totalAmount;
        totalKwhUsed = billing.totalKwhUsed;
        _maybeAddBillingHistoryEntry(billing);
      });
      logInfo(
          'Received billing update via MQTT: \$${billing.totalAmount.toStringAsFixed(2)}');
    };
    _mqttService.addBillingListener(_billingListener!);

    final connected = await _mqttService.connect(_automaticUserId);
    if (connected) {
      logInfo('Successfully connected to MQTT broker');
    } else {
      logError('Failed to connect to MQTT broker', null, null);
    }
  }

  void _startFetchingBilling() {
    _billingService.startFetchingBilling(
      _automaticUserId,
      onBillingUpdate: (billing) {
        setState(() {
          currentBilling = billing;
          totalBill = billing.totalAmount;
          totalKwhUsed = billing.totalKwhUsed;
          _maybeAddBillingHistoryEntry(billing);
        });
      },
    );
    logInfo('Started fetching billing data');
  }

  void _startAlertService() {
    _alertService.startListeningForAlerts(
      onAlertReceived: (alert) {
        logInfo('Received grid alert: ${alert.message}');
      },
      showOverlay: (alert) {
        _showAlertOverlay(alert);
      },
      enableMockAlerts: true,
    );
    logInfo('Started alert service with mock alerts enabled');
  }

  void _showAlertOverlay(alert) {
    // Use OverlayEntry to show a custom notification on desktop
    final overlay = Overlay.of(context);
    late OverlayEntry overlayEntry;

    overlayEntry = OverlayEntry(
      builder: (context) => Positioned(
        top: 20,
        right: 20,
        child: Material(
          elevation: 10,
          borderRadius: BorderRadius.circular(8),
          child: Container(
            constraints: const BoxConstraints(maxWidth: 400),
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AlertService.getColorForSeverity(alert.severity),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  AlertService.getIconForSeverity(alert.severity),
                  style: const TextStyle(fontSize: 24),
                ),
                const SizedBox(width: 12),
                Flexible(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        alert.message,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      if (alert.affectedArea != null) ...[
                        const SizedBox(height: 4),
                        Text(
                          alert.affectedArea!,
                          style: TextStyle(
                            color: Colors.white.withValues(alpha: 0.9),
                            fontSize: 14,
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
                const SizedBox(width: 8),
                IconButton(
                  icon: const Icon(Icons.close, color: Colors.white),
                  onPressed: () => overlayEntry.remove(),
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(),
                ),
              ],
            ),
          ),
        ),
      ),
    );

    overlay.insert(overlayEntry);

    // Auto-dismiss after 5 seconds
    Future.delayed(const Duration(seconds: 5), () {
      if (overlayEntry.mounted) {
        overlayEntry.remove();
      }
    });
  }

  void _maybeAddBillingHistoryEntry(BillingModel billing) {
    final alreadyRecorded = (_previousBillingTimestamp != null &&
            billing.lastUpdated.isAtSameMomentAs(_previousBillingTimestamp!)) ||
        (_previousReadingCount != null &&
            billing.readingCount == _previousReadingCount);

    if (!alreadyRecorded) {
      final prevKwh = _previousBillingTotalKwh ?? 0.0;
      final prevAmount = _previousBillingTotalAmount ?? 0.0;

      final deltaKwh = billing.totalKwhUsed - prevKwh;
      final deltaAmount = billing.totalAmount - prevAmount;

      final bool resetDetected =
          billing.readingCount == 0 || deltaKwh <= 0 || deltaAmount <= 0;

      final double entryKwh = resetDetected ? billing.totalKwhUsed : deltaKwh;
      final double entryAmount =
          resetDetected ? billing.totalAmount : deltaAmount;

      billUpdates.insert(
          0,
          BillUpdate(
            energyUsage: entryKwh.abs(),
            amount: entryAmount.abs(),
            timestamp: billing.lastUpdated,
          ));

      if (billUpdates.length > 20) {
        billUpdates.removeLast();
      }
    }

    _previousBillingTotalKwh = billing.totalKwhUsed;
    _previousBillingTotalAmount = billing.totalAmount;
    _previousBillingTimestamp = billing.lastUpdated;
    _previousReadingCount = billing.readingCount;
  }

  @override
  Widget build(BuildContext context) {
    logTrace('Build method called');

    try {
      return Scaffold(
        backgroundColor: const Color(0xFF0D0B1E),
        body: SafeArea(
          child: ConstrainedBox(
            constraints: const BoxConstraints(minWidth: 800, minHeight: 600),
            child: SingleChildScrollView(
              child: Container(
                height: MediaQuery.of(context).size.height,
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  children: [
                    // Top bar with connection status and total bill
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Left side - Connection status and countdown
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            ConnectionStatusWidget(isConnected: isConnected),
                            const SizedBox(height: 16),
                            if (isConnected && timeUntilNextReading != null)
                              NextReadingCountdownWidget(
                                timeUntilNext: timeUntilNextReading!,
                              )
                            else
                              Container(
                                padding: const EdgeInsets.all(16),
                                decoration: BoxDecoration(
                                  color: Colors.grey.shade800.withValues(
                                    alpha: 0.3,
                                  ),
                                  borderRadius: BorderRadius.circular(12),
                                ),
                                child: Text(
                                  'Next reading: --',
                                  style: TextStyle(
                                    color: Colors.grey.shade400,
                                    fontSize: 14,
                                  ),
                                ),
                              ),
                          ],
                        ),
                        // Right side - Total bill
                        TotalBillWidget(totalAmount: totalBill),
                      ],
                    ),
                    const SizedBox(height: 32),
                    // Main content - Bill updates list
                    Expanded(
                      child: BillUpdatesListWidget(updates: billUpdates),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      );
    } catch (error, stackTrace) {
      logError('Error building dashboard', error, stackTrace);
      return Scaffold(
        backgroundColor: const Color(0xFF0D0B1E),
        body: Center(
          child: Text(
            'Error loading dashboard',
            style: TextStyle(color: Colors.red.shade300),
          ),
        ),
      );
    }
  }
}
