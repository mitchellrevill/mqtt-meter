import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:red_footed_energy/presentation/widgets/dashboard/total_bill_widget.dart';

void main() {
  group('TotalBillWidget Tests', () {
    testWidgets('should display "--" when no bill amount is provided', (
      WidgetTester tester,
    ) async {
      // Given: A TotalBillWidget with no amount
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: TotalBillWidget(totalAmount: null)),
        ),
      );

      // Then: It should show placeholder text instead of a number
      expect(find.text('--'), findsOneWidget);
      expect(find.text('No data available'), findsOneWidget);
      expect(find.text('Total Bill'), findsOneWidget);
    });

    testWidgets(
      'should display the correct amount when bill data is available',
      (WidgetTester tester) async {
        // Given: A TotalBillWidget with a specific amount
        const double testAmount = 127.45;

        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(body: TotalBillWidget(totalAmount: testAmount)),
          ),
        );

        // Then: It should display the formatted amount
        expect(find.text('\$127.45'), findsOneWidget);
        expect(find.text('Total Bill'), findsOneWidget);
        expect(find.text('No data available'), findsNothing);
      },
    );

    testWidgets('should format amounts correctly with two decimal places', (
      WidgetTester tester,
    ) async {
      // Given: Various amounts that need proper formatting
      const testCases = [
        (0.0, '\$0.00'),
        (1.5, '\$1.50'),
        (999.99, '\$999.99'),
        (1000.0, '\$1000.00'),
      ];

      for (final (amount, expectedText) in testCases) {
        await tester.pumpWidget(
          MaterialApp(
            home: Scaffold(body: TotalBillWidget(totalAmount: amount)),
          ),
        );

        // Then: Each amount should be formatted correctly
        expect(
          find.text(expectedText),
          findsOneWidget,
          reason: 'Amount $amount should display as $expectedText',
        );
      }
    });

    testWidgets('should have proper styling when data is available', (
      WidgetTester tester,
    ) async {
      // Given: A widget with data
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: TotalBillWidget(totalAmount: 50.0)),
        ),
      );

      // When: We find the amount text widget
      final amountTextWidget = tester.widget<Text>(find.text('\$50.00'));

      // Then: It should have white color (indicating data is available)
      expect(amountTextWidget.style?.color, Colors.white);
      expect(amountTextWidget.style?.fontSize, 24);
      expect(amountTextWidget.style?.fontWeight, FontWeight.bold);
    });

    testWidgets('should have muted styling when no data is available', (
      WidgetTester tester,
    ) async {
      // Given: A widget with no data
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: TotalBillWidget(totalAmount: null)),
        ),
      );

      // When: We find the placeholder text widget
      final placeholderTextWidget = tester.widget<Text>(find.text('--'));

      // Then: It should have a muted gray color (indicating no data)
      expect(placeholderTextWidget.style?.color, Colors.grey.shade400);
      expect(placeholderTextWidget.style?.fontSize, 24);
      expect(placeholderTextWidget.style?.fontWeight, FontWeight.bold);
    });

    testWidgets('should have the correct container decoration', (
      WidgetTester tester,
    ) async {
      // Given: Any TotalBillWidget
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: TotalBillWidget(totalAmount: 100.0)),
        ),
      );

      // When: We find the main container
      final containerWidget = tester.widget<Container>(find.byType(Container));
      final decoration = containerWidget.decoration as BoxDecoration;

      // Then: It should have the proper purple gradient and styling
      expect(decoration.gradient, isA<LinearGradient>());
      expect(decoration.borderRadius, BorderRadius.circular(12));
      expect(decoration.boxShadow, isNotNull);
      expect(decoration.boxShadow!.length, 1);
    });

    testWidgets('should display help text only when no data is available', (
      WidgetTester tester,
    ) async {
      // Test case 1: With data - no help text
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: TotalBillWidget(totalAmount: 42.0)),
        ),
      );

      expect(find.text('No data available'), findsNothing);

      // Test case 2: Without data - help text appears
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(body: TotalBillWidget(totalAmount: null)),
        ),
      );

      expect(find.text('No data available'), findsOneWidget);
    });

    testWidgets('should handle edge cases gracefully', (
      WidgetTester tester,
    ) async {
      // Given: Edge case amounts
      const edgeCases = [
        (0.0, 'zero amount'),
        (-1.50, 'negative amount'),
        (999999.99, 'very large amount'),
      ];

      for (final (amount, description) in edgeCases) {
        await tester.pumpWidget(
          MaterialApp(
            home: Scaffold(body: TotalBillWidget(totalAmount: amount)),
          ),
        );

        // Then: Widget should render without crashing
        expect(
          find.byType(TotalBillWidget),
          findsOneWidget,
          reason: 'Widget should handle $description',
        );
        expect(
          find.text('Total Bill'),
          findsOneWidget,
          reason: 'Title should always be visible for $description',
        );
      }
    });
  });
}
