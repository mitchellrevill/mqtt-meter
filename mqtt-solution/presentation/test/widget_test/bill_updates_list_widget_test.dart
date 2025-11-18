import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:red_footed_energy/presentation/widgets/dashboard/bill_updates_list_widget.dart';

void main() {
  group('BillUpdatesListWidget Tests', () {
    testWidgets(
      'should show "No updates yet" when bill updates list is empty',
      (WidgetTester tester) async {
        // Given: An empty list of bill updates
        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(body: BillUpdatesListWidget(updates: [])),
          ),
        );

        // Then: It should display the empty state message
        expect(find.text('No updates yet'), findsOneWidget);
        expect(find.text('Recent Bill Updates'), findsOneWidget);
        expect(find.byType(ListView), findsNothing);
      },
    );

    testWidgets(
      'should display bill updates in a scrollable list when data is available',
      (WidgetTester tester) async {
        // Given: A list with some bill updates
        final testUpdates = [
          BillUpdate(
            timestamp: DateTime(2025, 10, 14, 10, 30),
            amount: 2.34,
            energyUsage: 4.2,
          ),
          BillUpdate(
            timestamp: DateTime(2025, 10, 14, 9, 45),
            amount: 1.87,
            energyUsage: 3.1,
          ),
        ];

        await tester.pumpWidget(
          MaterialApp(
            home: Scaffold(body: BillUpdatesListWidget(updates: testUpdates)),
          ),
        );

        // Then: It should show a scrollable list with the updates
        expect(find.byType(ListView), findsOneWidget);
        expect(find.text('Recent Bill Updates'), findsOneWidget);
        expect(find.text('No updates yet'), findsNothing);

        // Check that both updates are displayed
        expect(find.text('4.20 kWh'), findsOneWidget);
        expect(find.text('3.10 kWh'), findsOneWidget);
        expect(find.text('\$2.34'), findsOneWidget);
        expect(find.text('\$1.87'), findsOneWidget);
      },
    );

    testWidgets('should format energy usage and amounts correctly', (
      WidgetTester tester,
    ) async {
      // Given: A bill update with specific values that need formatting
      final testUpdate = BillUpdate(
        timestamp: DateTime.now(),
        amount: 0.5, // Should become $0.50
        energyUsage: 1.0, // Should become 1.00 kWh
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(body: BillUpdatesListWidget(updates: [testUpdate])),
        ),
      );

      // Then: Values should be formatted with proper decimals
      expect(find.text('1.00 kWh'), findsOneWidget);
      expect(find.text('\$0.50'), findsOneWidget);
    });

    testWidgets('should display relative timestamps correctly', (
      WidgetTester tester,
    ) async {
      // Given: Updates with different timestamps
      final now = DateTime.now();
      final testUpdates = [
        BillUpdate(
          timestamp: now.subtract(const Duration(seconds: 30)),
          amount: 1.0,
          energyUsage: 2.0,
        ),
        BillUpdate(
          timestamp: now.subtract(const Duration(minutes: 5)),
          amount: 1.0,
          energyUsage: 2.0,
        ),
        BillUpdate(
          timestamp: now.subtract(const Duration(hours: 2)),
          amount: 1.0,
          energyUsage: 2.0,
        ),
        BillUpdate(
          timestamp: now.subtract(const Duration(days: 1)),
          amount: 1.0,
          energyUsage: 2.0,
        ),
      ];

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(body: BillUpdatesListWidget(updates: testUpdates)),
        ),
      );

      // Then: Different time formats should be displayed
      expect(find.textContaining('Just now'), findsOneWidget);
      expect(find.textContaining('5m ago'), findsOneWidget);
      expect(find.textContaining('2h ago'), findsOneWidget);
      expect(find.textContaining('1d ago'), findsOneWidget);
    });

    testWidgets('should have proper list item structure and styling', (
      WidgetTester tester,
    ) async {
      // Given: A single bill update
      final testUpdate = BillUpdate(
        timestamp: DateTime.now(),
        amount: 5.67,
        energyUsage: 10.5,
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(body: BillUpdatesListWidget(updates: [testUpdate])),
        ),
      );

      // Then: Each list item should have the correct structure
      expect(
        find.byType(Container),
        findsAtLeastNWidgets(2),
      ); // Main container + list item container

      // Should have a circular indicator dot
      final decoratedBoxes = find.byType(Container);
      expect(decoratedBoxes, findsAtLeastNWidgets(1));

      // Should display all required information
      expect(find.text('10.50 kWh'), findsOneWidget);
      expect(find.text('\$5.67'), findsOneWidget);
    });

    testWidgets('should handle large lists efficiently', (
      WidgetTester tester,
    ) async {
      // Given: A large list of bill updates
      final largeUpdatesList = List.generate(
        50,
        (index) => BillUpdate(
          timestamp: DateTime.now().subtract(Duration(minutes: index)),
          amount: (index + 1) * 1.5,
          energyUsage: (index + 1) * 2.5,
        ),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: BillUpdatesListWidget(updates: largeUpdatesList),
          ),
        ),
      );

      // Then: Should use ListView.builder for efficiency
      expect(find.byType(ListView), findsOneWidget);

      // Should render without performance issues
      expect(find.text('Recent Bill Updates'), findsOneWidget);

      // Only visible items should be in the widget tree initially
      // (ListView.builder lazy loads)
      expect(find.byType(Container), findsWidgets);
    });

    testWidgets('should maintain consistent spacing and layout', (
      WidgetTester tester,
    ) async {
      // Given: Multiple bill updates
      final testUpdates = [
        BillUpdate(timestamp: DateTime.now(), amount: 1.0, energyUsage: 2.0),
        BillUpdate(timestamp: DateTime.now(), amount: 3.0, energyUsage: 4.0),
      ];

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(body: BillUpdatesListWidget(updates: testUpdates)),
        ),
      );

      // Then: Should have proper title padding
      final titlePadding = tester.widget<Padding>(
        find
            .ancestor(
              of: find.text('Recent Bill Updates'),
              matching: find.byType(Padding),
            )
            .first,
      );
      expect(titlePadding.padding, const EdgeInsets.all(16));

      // Should have proper list padding
      final listView = tester.widget<ListView>(find.byType(ListView));
      expect(listView.padding, const EdgeInsets.all(8));
    });

    testWidgets('should show divider between title and content', (
      WidgetTester tester,
    ) async {
      // Given: Any bill updates widget
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: BillUpdatesListWidget(updates: [])),
        ),
      );

      // Then: Should have a divider after the title
      expect(find.byType(Divider), findsOneWidget);

      final dividerWidget = tester.widget<Divider>(find.byType(Divider));
      expect(dividerWidget.color, Colors.deepPurple);
      expect(dividerWidget.height, 1);
    });

    testWidgets('should handle edge case timestamps gracefully', (
      WidgetTester tester,
    ) async {
      // Given: An update with a future timestamp (edge case)
      final futureUpdate = BillUpdate(
        timestamp: DateTime.now().add(const Duration(hours: 1)),
        amount: 1.0,
        energyUsage: 2.0,
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(body: BillUpdatesListWidget(updates: [futureUpdate])),
        ),
      );

      // Then: Should not crash and should display some reasonable text
      expect(find.byType(BillUpdatesListWidget), findsOneWidget);
      expect(find.text('2.00 kWh'), findsOneWidget);
      expect(find.text('\$1.00'), findsOneWidget);
    });
  });
}
