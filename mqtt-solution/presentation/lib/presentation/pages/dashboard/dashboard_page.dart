import 'package:flutter/material.dart';
import '../../widgets/dashboard/connection_status_widget.dart';
import '../../widgets/dashboard/total_bill_widget.dart';
import '../../widgets/dashboard/next_reading_countdown_widget.dart';
import '../../widgets/dashboard/bill_updates_list_widget.dart';

class DashboardPage extends StatefulWidget {
  const DashboardPage({super.key});

  @override
  State<DashboardPage> createState() => _DashboardPageState();
}

class _DashboardPageState extends State<DashboardPage> {
  // Mock data - will be replaced with real data later
  bool isConnected = true;
  double totalBill = 127.45;
  Duration timeUntilNextReading = const Duration(minutes: 4, seconds: 32);

  List<BillUpdate> billUpdates = [
    BillUpdate(
      timestamp: DateTime.now().subtract(const Duration(minutes: 2)),
      amount: 2.34,
      energyUsage: 4.2,
    ),
    BillUpdate(
      timestamp: DateTime.now().subtract(const Duration(minutes: 15)),
      amount: 1.87,
      energyUsage: 3.1,
    ),
    BillUpdate(
      timestamp: DateTime.now().subtract(const Duration(hours: 1)),
      amount: 3.12,
      energyUsage: 5.8,
    ),
    BillUpdate(
      timestamp: DateTime.now().subtract(const Duration(hours: 2)),
      amount: 1.45,
      energyUsage: 2.7,
    ),
  ];

  @override
  Widget build(BuildContext context) {
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
                          NextReadingCountdownWidget(
                            timeUntilNext: timeUntilNextReading,
                          ),
                        ],
                      ),
                      // Right side - Total bill
                      TotalBillWidget(totalAmount: totalBill),
                    ],
                  ),
                  const SizedBox(height: 32),
                  // Main content - Bill updates list
                  Expanded(child: BillUpdatesListWidget(updates: billUpdates)),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
