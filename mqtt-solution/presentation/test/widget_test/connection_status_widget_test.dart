import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:red_footed_energy/presentation/widgets/dashboard/connection_status_widget.dart';

void main() {
  group('ConnectionStatusWidget Tests', () {
    testWidgets(
      'should show "Connected" with green styling when connection is active',
      (WidgetTester tester) async {
        // Given: A connected status
        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(body: ConnectionStatusWidget(isConnected: true)),
          ),
        );

        // Then: It should display connected state with proper styling
        expect(find.text('Connected'), findsOneWidget);
        expect(find.byIcon(Icons.wifi), findsOneWidget);
        expect(find.byIcon(Icons.wifi_off), findsNothing);

        // Check that the text has green color
        final textWidget = tester.widget<Text>(find.text('Connected'));
        expect(textWidget.style?.color, Colors.green);
        expect(textWidget.style?.fontWeight, FontWeight.bold);
      },
    );

    testWidgets(
      'should show "Disconnected" with red styling when connection is lost',
      (WidgetTester tester) async {
        // Given: A disconnected status
        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(body: ConnectionStatusWidget(isConnected: false)),
          ),
        );

        // Then: It should display disconnected state with proper styling
        expect(find.text('Disconnected'), findsOneWidget);
        expect(find.byIcon(Icons.wifi_off), findsOneWidget);
        expect(find.byIcon(Icons.wifi), findsNothing);

        // Check that the text has red color
        final textWidget = tester.widget<Text>(find.text('Disconnected'));
        expect(textWidget.style?.color, Colors.red);
        expect(textWidget.style?.fontWeight, FontWeight.bold);
      },
    );

    testWidgets('should have correct container styling for connected state', (
      WidgetTester tester,
    ) async {
      // Given: A connected widget
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: ConnectionStatusWidget(isConnected: true)),
        ),
      );

      // When: We examine the container
      final containerWidget = tester.widget<Container>(find.byType(Container));
      final decoration = containerWidget.decoration as BoxDecoration;

      // Then: It should have green theming
      expect(decoration.color, Colors.green.withValues(alpha: 0.2));
      expect(decoration.border?.top.color, Colors.green);
      expect(decoration.borderRadius, BorderRadius.circular(8));
    });

    testWidgets(
      'should have correct container styling for disconnected state',
      (WidgetTester tester) async {
        // Given: A disconnected widget
        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(body: ConnectionStatusWidget(isConnected: false)),
          ),
        );

        // When: We examine the container
        final containerWidget = tester.widget<Container>(
          find.byType(Container),
        );
        final decoration = containerWidget.decoration as BoxDecoration;

        // Then: It should have red theming
        expect(decoration.color, Colors.red.withValues(alpha: 0.2));
        expect(decoration.border?.top.color, Colors.red);
        expect(decoration.borderRadius, BorderRadius.circular(8));
      },
    );

    testWidgets('should display icon and text in correct order', (
      WidgetTester tester,
    ) async {
      // Given: Any connection status widget
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: ConnectionStatusWidget(isConnected: true)),
        ),
      );

      // When: We find the row widget
      final rowWidget = tester.widget<Row>(find.byType(Row));

      // Then: Icon should come before text (first and third children, with SizedBox in between)
      expect(rowWidget.children.length, 3);
      expect(rowWidget.children[0], isA<Icon>());
      expect(rowWidget.children[1], isA<SizedBox>());
      expect(rowWidget.children[2], isA<Text>());
    });

    testWidgets('should have consistent spacing between icon and text', (
      WidgetTester tester,
    ) async {
      // Given: Any connection status widget
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: ConnectionStatusWidget(isConnected: false)),
        ),
      );

      // When: We find the SizedBox between icon and text (inside the Row)
      final rowWidget = tester.widget<Row>(find.byType(Row));
      final sizedBoxWidget = rowWidget.children[1] as SizedBox;

      // Then: It should have consistent width spacing
      expect(sizedBoxWidget.width, 8.0);
    });

    testWidgets('should handle state changes properly', (
      WidgetTester tester,
    ) async {
      // Given: A stateful parent widget that can change connection status
      bool isConnected = false;

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: StatefulBuilder(
              builder: (context, setState) {
                return Column(
                  children: [
                    ConnectionStatusWidget(isConnected: isConnected),
                    ElevatedButton(
                      onPressed: () =>
                          setState(() => isConnected = !isConnected),
                      child: const Text('Toggle Connection'),
                    ),
                  ],
                );
              },
            ),
          ),
        ),
      );

      // Initially disconnected
      expect(find.text('Disconnected'), findsOneWidget);
      expect(find.byIcon(Icons.wifi_off), findsOneWidget);

      // When: We toggle the connection
      await tester.tap(find.text('Toggle Connection'));
      await tester.pump();

      // Then: It should update to connected state
      expect(find.text('Connected'), findsOneWidget);
      expect(find.byIcon(Icons.wifi), findsOneWidget);
      expect(find.text('Disconnected'), findsNothing);
    });

    testWidgets('should have proper icon sizing', (WidgetTester tester) async {
      // Given: Both connected and disconnected states
      for (final isConnected in [true, false]) {
        await tester.pumpWidget(
          MaterialApp(
            home: Scaffold(
              body: ConnectionStatusWidget(isConnected: isConnected),
            ),
          ),
        );

        // When: We find the icon
        final iconWidget = tester.widget<Icon>(find.byType(Icon));

        // Then: It should have consistent sizing
        expect(iconWidget.size, 16.0);
        expect(iconWidget.color, isConnected ? Colors.green : Colors.red);
      }
    });
  });
}
