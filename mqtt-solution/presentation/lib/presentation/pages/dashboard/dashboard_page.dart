import 'package:flutter/material.dart';
import '../../widgets/dashboard/connection_status_widget.dart';
import '../../widgets/dashboard/total_bill_widget.dart';
import '../../widgets/dashboard/next_reading_countdown_widget.dart';
import '../../widgets/dashboard/bill_updates_list_widget.dart';
import '../../../data/services/meter_reading_service.dart';
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

  // Meter reading service
  final MeterReadingService _meterService = MeterReadingService();

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

    // Automatically start meter readings when the app launches
    _startAutomaticMeterReadings();

    logInfo('Dashboard initialization complete');
  }

  @override
  void dispose() {
    _meterService.stopSendingReadings();
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

    logInfo('Automatically started meter readings for user: $_automaticUserId');
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
