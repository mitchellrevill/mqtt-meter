import '../utils/app_logger.dart';

/// A mixin that provides easy logging capabilities to any class
mixin LoggerMixin {
  String get loggerTag => runtimeType.toString();

  void logTrace(String message, [dynamic error, StackTrace? stackTrace]) {
    AppLogger.trace('[$loggerTag] $message', error, stackTrace);
  }

  void logDebug(String message, [dynamic error, StackTrace? stackTrace]) {
    AppLogger.debug('[$loggerTag] $message', error, stackTrace);
  }

  void logInfo(String message, [dynamic error, StackTrace? stackTrace]) {
    AppLogger.info('[$loggerTag] $message', error, stackTrace);
  }

  void logWarning(String message, [dynamic error, StackTrace? stackTrace]) {
    AppLogger.warning('[$loggerTag] $message', error, stackTrace);
  }

  void logError(String message, [dynamic error, StackTrace? stackTrace]) {
    AppLogger.error('[$loggerTag] $message', error, stackTrace);
  }

  void logFatal(String message, [dynamic error, StackTrace? stackTrace]) {
    AppLogger.fatal('[$loggerTag] $message', error, stackTrace);
  }
}

/// Extension methods for quick logging
extension QuickLogging on Object {
  void log(String message) {
    AppLogger.info('[$runtimeType] $message');
  }

  void logError(String message, [dynamic error, StackTrace? stackTrace]) {
    AppLogger.error('[$runtimeType] $message', error, stackTrace);
  }
}
