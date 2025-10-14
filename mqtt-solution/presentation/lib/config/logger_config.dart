import '../core/utils/app_logger.dart';
import '../core/constants/logging_constants.dart';

class LoggerConfig {
  static void initializeLogger({bool isDebugMode = true}) {
    if (isDebugMode) {
      // Development configuration - more verbose logging
      AppLogger.configure(
        level: LogLevel.debug,
        loggerName: LoggingConstants.loggerName,
        showTime: true,
        showEmojis: true,
        showColors: true,
      );

      AppLogger.info('${LoggingConstants.appStarted} - Debug Mode');
    } else {
      // Production configuration - less verbose logging
      AppLogger.configure(
        level: LogLevel.warning,
        loggerName: LoggingConstants.loggerName,
        showTime: true,
        showEmojis: false,
        showColors: false,
      );

      AppLogger.info('${LoggingConstants.appStarted} - Production Mode');
    }
  }

  static void logAppLifecycle(String event) {
    AppLogger.info('[${LoggingConstants.categoryUI}] App lifecycle: $event');
  }

  static void logDashboardEvent(String event) {
    AppLogger.info('[${LoggingConstants.categoryDashboard}] $event');
  }

  static void logAPICall(String endpoint, {String? method}) {
    AppLogger.debug(
      '[${LoggingConstants.categoryAPI}] ${method ?? 'GET'} $endpoint',
    );
  }

  static void logMQTTEvent(String event) {
    AppLogger.info('[${LoggingConstants.categoryMQTT}] $event');
  }

  static void logBillingEvent(String event, {double? amount}) {
    final message =
        amount != null
            ? '$event - Amount: \$${amount.toStringAsFixed(2)}'
            : event;
    AppLogger.info('[${LoggingConstants.categoryBilling}] $message');
  }

  static void logError(
    String message, [
    dynamic error,
    StackTrace? stackTrace,
  ]) {
    AppLogger.error(
      '[${LoggingConstants.categoryError}] $message',
      error,
      stackTrace,
    );
  }
}
