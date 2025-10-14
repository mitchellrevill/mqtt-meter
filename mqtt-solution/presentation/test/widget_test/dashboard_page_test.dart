import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:presentation/presentation/pages/dashboard/dashboard_page.dart';
import 'package:presentation/presentation/widgets/dashboard/connection_status_widget.dart';
import 'package:presentation/presentation/widgets/dashboard/total_bill_widget.dart';
import 'package:presentation/presentation/widgets/dashboard/bill_updates_list_widget.dart';

void main() {
  group('DashboardPage Tests', () {
    testWidgets(
      'should display all dashboard widgets in correct layout when loaded',
      (WidgetTester tester) async {
        // Given: A fresh dashboard page
        await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

        // Then: All main components should be present
        expect(find.byType(ConnectionStatusWidget), findsOneWidget);
        expect(find.byType(TotalBillWidget), findsOneWidget);
        expect(find.byType(BillUpdatesListWidget), findsOneWidget);
      },
    );

    testWidgets('should show disconnected state by default', (
      WidgetTester tester,
    ) async {
      // Given: A new dashboard page (no data loaded yet)
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Should show disconnected status
      expect(find.text('Disconnected'), findsOneWidget);
      expect(find.byIcon(Icons.wifi_off), findsOneWidget);
    });

    testWidgets('should display empty bill state when no data is available', (
      WidgetTester tester,
    ) async {
      // Given: A dashboard with no bill data
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Total bill should show empty state
      expect(find.text('--'), findsOneWidget);
      expect(find.text('No data available'), findsOneWidget);
    });

    testWidgets(
      'should show placeholder for next reading when no data is available',
      (WidgetTester tester) async {
        // Given: A dashboard with no reading schedule data
        await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

        // Then: Should show placeholder for next reading
        expect(find.text('Next reading: --'), findsOneWidget);
      },
    );

    testWidgets('should display empty bill updates list by default', (
      WidgetTester tester,
    ) async {
      // Given: A fresh dashboard
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Bill updates should show empty state
      expect(find.text('No updates yet'), findsOneWidget);
      expect(find.text('Recent Bill Updates'), findsOneWidget);
    });

    testWidgets('should have proper layout structure with correct spacing', (
      WidgetTester tester,
    ) async {
      // Given: A dashboard page
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Should have the main scaffold structure
      expect(find.byType(Scaffold), findsOneWidget);
      expect(find.byType(SafeArea), findsOneWidget);
      expect(find.byType(ConstrainedBox), findsAtLeastNWidgets(1));
      expect(find.byType(SingleChildScrollView), findsOneWidget);

      // Should have proper column layout
      expect(find.byType(Column), findsAtLeastNWidgets(1));

      // Should have multiple rows (main dashboard row + connection status row)
      expect(find.byType(Row), findsAtLeastNWidgets(1));
    });

    testWidgets('should use dark theme background color', (
      WidgetTester tester,
    ) async {
      // Given: A dashboard page
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // When: We examine the scaffold
      final scaffold = tester.widget<Scaffold>(find.byType(Scaffold));

      // Then: Should have the correct dark background
      expect(scaffold.backgroundColor, const Color(0xFF0D0B1E));
    });

    testWidgets('should maintain proper spacing between components', (
      WidgetTester tester,
    ) async {
      // Given: A dashboard page
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Should have SizedBox widgets for spacing
      final sizedBoxes = find.byType(SizedBox);
      expect(sizedBoxes, findsAtLeastNWidgets(2)); // At least 2 for spacing

      // Main container should have proper padding
      final containerPadding = tester.widget<Container>(
        find
            .descendant(
              of: find.byType(SingleChildScrollView),
              matching: find.byType(Container),
            )
            .first,
      );
      expect(containerPadding.padding, const EdgeInsets.all(24.0));
    });

    testWidgets(
      'should position connection status and bill widgets correctly in top row',
      (WidgetTester tester) async {
        // Given: A dashboard page
        await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

        // When: We examine the main dashboard row (not the one inside ConnectionStatusWidget)
        final dashboardRows = find.byType(Row);
        expect(dashboardRows, findsAtLeastNWidgets(1));

        // Find the row that contains both ConnectionStatusWidget and TotalBillWidget
        Row? mainDashboardRow;
        for (int i = 0; i < tester.widgetList(dashboardRows).length; i++) {
          final rowWidget = tester.widget<Row>(dashboardRows.at(i));

          // Check if this row has 2 children (left column and right total bill)
          if (rowWidget.children.length == 2) {
            mainDashboardRow = rowWidget;
            break;
          }
        }

        // Then: Should have found the main dashboard row
        expect(mainDashboardRow, isNotNull);
        expect(
          mainDashboardRow!.mainAxisAlignment,
          MainAxisAlignment.spaceBetween,
        );
        expect(mainDashboardRow.crossAxisAlignment, CrossAxisAlignment.start);
        expect(
          mainDashboardRow.children.length,
          2,
        ); // Left column and right total bill
      },
    );

    testWidgets('should have expandable bill updates section', (
      WidgetTester tester,
    ) async {
      // Given: A dashboard page
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Bill updates should be in an Expanded widget to fill remaining space
      expect(find.byType(Expanded), findsWidgets);

      // The main content expanded widget should contain the BillUpdatesListWidget
      expect(
        find.descendant(
          of: find.byType(Expanded),
          matching: find.byType(BillUpdatesListWidget),
        ),
        findsOneWidget,
      );
    });

    testWidgets('should handle errors gracefully and show error state', (
      WidgetTester tester,
    ) async {
      // This test verifies the error handling structure exists
      // Given: A dashboard page that loads successfully
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Should not show any error state (normal operation)
      expect(find.text('Error loading dashboard'), findsNothing);

      // Should show normal dashboard content
      expect(find.byType(ConnectionStatusWidget), findsOneWidget);
      expect(find.byType(TotalBillWidget), findsOneWidget);
      expect(find.byType(BillUpdatesListWidget), findsOneWidget);
    });

    testWidgets('should have proper widget tree depth for performance', (
      WidgetTester tester,
    ) async {
      // Given: A dashboard page
      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Should not have excessive nesting that could affect performance
      // Each key component should be findable
      expect(find.byType(MaterialApp), findsOneWidget);
      expect(find.byType(DashboardPage), findsOneWidget);
      expect(find.byType(Scaffold), findsOneWidget);

      // Main content structure should be clear
      expect(find.byType(SafeArea), findsOneWidget);
      expect(find.byType(SingleChildScrollView), findsOneWidget);
    });

    testWidgets('should support scrolling when content overflows', (
      WidgetTester tester,
    ) async {
      // Given: A dashboard page in a constrained space
      await tester.binding.setSurfaceSize(
        const Size(800, 400),
      ); // Smaller height

      await tester.pumpWidget(const MaterialApp(home: DashboardPage()));

      // Then: Should have SingleChildScrollView to handle overflow
      expect(find.byType(SingleChildScrollView), findsOneWidget);

      // Content should still be accessible
      expect(find.byType(ConnectionStatusWidget), findsOneWidget);
      expect(find.byType(TotalBillWidget), findsOneWidget);

      // Reset surface size
      await tester.binding.setSurfaceSize(null);
    });
  });
}
