class LoggingConstants {
  static const String appName = 'MQTT Meter';
  static const String loggerName = 'MQTTMeterLogger';

  // Log categories
  static const String categoryDashboard = 'Dashboard';
  static const String categoryAPI = 'API';
  static const String categoryMQTT = 'MQTT';
  static const String categoryBilling = 'Billing';
  static const String categoryUI = 'UI';
  static const String categoryError = 'Error';

  // Common log messages
  static const String appStarted = 'Application started';
  static const String appStopped = 'Application stopped';
  static const String dashboardInitialized = 'Dashboard initialized';
  static const String connectionEstablished = 'Connection established';
  static const String connectionLost = 'Connection lost';
  static const String dataReceived = 'Data received';
  static const String errorOccurred = 'Error occurred';
}
