import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:red_footed_energy/presentation/widgets/dashboard/next_reading_countdown_widget.dart';

void main() {
  group('NextReadingCountdownWidget Tests', () {
    testWidgets(
      'should display countdown timer with proper formatting when time is provided',
      (WidgetTester tester) async {
        // Given: A countdown widget with 5 minutes and 30 seconds remaining
        const testDuration = Duration(minutes: 5, seconds: 30);

        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(
              body: NextReadingCountdownWidget(timeUntilNext: testDuration),
            ),
          ),
        );

        // Then: It should display properly formatted time
        expect(find.text('Next Reading In'), findsOneWidget);
        expect(find.text('05:30'), findsOneWidget);

        // And: Should have the correct styling
        final titleText = tester.widget<Text>(find.text('Next Reading In'));
        expect(titleText.style?.fontSize, 14);
        expect(titleText.style?.fontWeight, FontWeight.w500);

        final timeText = tester.widget<Text>(find.text('05:30'));
        expect(timeText.style?.fontSize, 20);
        expect(timeText.style?.fontWeight, FontWeight.bold);
        expect(timeText.style?.fontFamily, 'monospace');
        expect(timeText.style?.color, Colors.white);
      },
    );

    testWidgets(
      'should display zero padding for single digit minutes and seconds',
      (WidgetTester tester) async {
        // Given: A countdown widget with 3 minutes and 7 seconds remaining
        const testDuration = Duration(minutes: 3, seconds: 7);

        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(
              body: NextReadingCountdownWidget(timeUntilNext: testDuration),
            ),
          ),
        );

        // Then: Should display with proper zero padding
        expect(find.text('03:07'), findsOneWidget);
        expect(
          find.text('3:7'),
          findsNothing,
        ); // Should not find unpadded version
      },
    );

    testWidgets('should handle zero time remaining gracefully', (
      WidgetTester tester,
    ) async {
      // Given: A countdown widget with no time remaining
      const testDuration = Duration.zero;

      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(
            body: NextReadingCountdownWidget(timeUntilNext: testDuration),
          ),
        ),
      );

      // Then: Should display 00:00
      expect(find.text('00:00'), findsOneWidget);
      expect(find.text('Next Reading In'), findsOneWidget);
    });

    testWidgets(
      'should handle large durations by showing only minutes and seconds',
      (WidgetTester tester) async {
        // Given: A countdown widget with over an hour remaining (75 minutes)
        const testDuration = Duration(hours: 1, minutes: 15, seconds: 45);

        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(
              body: NextReadingCountdownWidget(timeUntilNext: testDuration),
            ),
          ),
        );

        // Then: Should display total minutes (75) and seconds
        expect(find.text('75:45'), findsOneWidget);
        expect(find.text('Next Reading In'), findsOneWidget);
      },
    );

    testWidgets('should have proper container styling and decoration', (
      WidgetTester tester,
    ) async {
      // Given: Any countdown widget
      const testDuration = Duration(minutes: 2, seconds: 15);

      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(
            body: NextReadingCountdownWidget(timeUntilNext: testDuration),
          ),
        ),
      );

      // When: We examine the container decoration
      final containerWidget = tester.widget<Container>(find.byType(Container));
      final decoration = containerWidget.decoration as BoxDecoration;

      // Then: Should have proper styling
      expect(containerWidget.padding, const EdgeInsets.all(16));
      expect(
        decoration.color,
        Colors.deepPurple.shade800.withValues(alpha: 0.6),
      );
      expect(decoration.borderRadius, BorderRadius.circular(12));
      expect(decoration.border?.top.color, Colors.deepPurple.shade400);
      expect(decoration.border?.top.width, 1);
    });

    testWidgets(
      'should display title and countdown in vertical column layout',
      (WidgetTester tester) async {
        // Given: A countdown widget
        const testDuration = Duration(minutes: 1, seconds: 30);

        await tester.pumpWidget(
          const MaterialApp(
            home: Scaffold(
              body: NextReadingCountdownWidget(timeUntilNext: testDuration),
            ),
          ),
        );

        // When: We examine the layout structure
        final columnWidget = tester.widget<Column>(find.byType(Column));

        // Then: Should have proper vertical layout
        expect(columnWidget.children.length, 3); // Title, SizedBox, Time
        expect(columnWidget.children[0], isA<Text>()); // Title
        expect(columnWidget.children[1], isA<SizedBox>()); // Spacing
        expect(columnWidget.children[2], isA<Text>()); // Time display

        // And: Should have proper spacing between elements
        final spacingWidget = columnWidget.children[1] as SizedBox;
        expect(spacingWidget.height, 8);
      },
    );

    testWidgets('should handle exactly 60 seconds as 1 minute 0 seconds', (
      WidgetTester tester,
    ) async {
      // Given: A countdown widget with exactly 60 seconds
      const testDuration = Duration(seconds: 60);

      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(
            body: NextReadingCountdownWidget(timeUntilNext: testDuration),
          ),
        ),
      );

      // Then: Should display as 01:00 (1 minute, 0 seconds)
      expect(find.text('01:00'), findsOneWidget);
    });

    testWidgets('should handle edge case of 59 seconds correctly', (
      WidgetTester tester,
    ) async {
      // Given: A countdown widget with 59 seconds
      const testDuration = Duration(seconds: 59);

      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(
            body: NextReadingCountdownWidget(timeUntilNext: testDuration),
          ),
        ),
      );

      // Then: Should display as 00:59
      expect(find.text('00:59'), findsOneWidget);
    });

    testWidgets('should use consistent purple theme colors throughout widget', (
      WidgetTester tester,
    ) async {
      // Given: A countdown widget
      const testDuration = Duration(minutes: 5);

      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(
            body: NextReadingCountdownWidget(timeUntilNext: testDuration),
          ),
        ),
      );

      // When: We examine the container and text styling
      final containerWidget = tester.widget<Container>(find.byType(Container));
      final decoration = containerWidget.decoration as BoxDecoration;

      final titleText = tester.widget<Text>(find.text('Next Reading In'));
      final timeText = tester.widget<Text>(find.text('05:00'));

      // Then: Should use consistent purple theming
      expect(
        decoration.color,
        Colors.deepPurple.shade800.withValues(alpha: 0.6),
      );
      expect(decoration.border?.top.color, Colors.deepPurple.shade400);
      expect(titleText.style?.color, Colors.grey.shade300);
      expect(timeText.style?.color, Colors.white);
    });

    testWidgets(
      'should maintain readability with monospace font for time display',
      (WidgetTester tester) async {
        // Given: Various time durations
        final testDurations = [
          const Duration(minutes: 1, seconds: 1), // 01:01
          const Duration(minutes: 11, seconds: 11), // 11:11
          const Duration(minutes: 0, seconds: 0), // 00:00
        ];

        for (final duration in testDurations) {
          await tester.pumpWidget(
            MaterialApp(
              home: Scaffold(
                body: NextReadingCountdownWidget(timeUntilNext: duration),
              ),
            ),
          );

          // When: We examine the time text widget
          final timeTextFinder = find.byWidgetPredicate(
            (widget) =>
                widget is Text &&
                widget.data != 'Next Reading In' &&
                widget.style?.fontFamily == 'monospace',
          );

          // Then: Should use monospace font for consistent character width
          expect(timeTextFinder, findsOneWidget);

          final timeTextWidget = tester.widget<Text>(timeTextFinder);
          expect(timeTextWidget.style?.fontFamily, 'monospace');
          expect(timeTextWidget.style?.fontWeight, FontWeight.bold);
        }
      },
    );
  });
}
