import 'package:flutter/material.dart';
import 'presentation/pages/dashboard/dashboard_page.dart';

void main() {
  runApp(const MainApp());
}

class MainApp extends StatelessWidget {
  const MainApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
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
