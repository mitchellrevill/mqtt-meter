import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'presentation/pages/dashboard/dashboard_page.dart';
import 'config/logger_config.dart';

void main() {
  // Initialize logger based on build mode
  LoggerConfig.initializeLogger(isDebugMode: kDebugMode);

  LoggerConfig.logAppLifecycle('Main function started');

  runApp(const MainApp());
}

class MainApp extends StatelessWidget {
  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    LoggerConfig.logAppLifecycle('MainApp build started');

    return MaterialApp(
      debugShowCheckedModeBanner: false,
      home: const DashboardPage(),
      builder: (context, child) {
        return ConstrainedBox(
          constraints: const BoxConstraints(minWidth: 800, minHeight: 600),
          child: child!,
        );
      },
    );
  }
}
