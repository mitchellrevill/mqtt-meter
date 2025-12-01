/// Model for grid alert notifications
/// These alerts notify users of electricity grid issues
class GridAlertModel {
  final String alertId;
  final String message;
  final AlertSeverity severity;
  final DateTime timestamp;
  final String? affectedArea;

  GridAlertModel({
    required this.alertId,
    required this.message,
    required this.severity,
    required this.timestamp,
    this.affectedArea,
  });

  factory GridAlertModel.fromJson(Map<String, dynamic> json) {
    return GridAlertModel(
      alertId: json['alertId'] as String,
      message: json['message'] as String,
      severity: AlertSeverity.fromString(json['severity'] as String),
      timestamp: DateTime.parse(json['timestamp'] as String),
      affectedArea: json['affectedArea'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'alertId': alertId,
      'message': message,
      'severity': severity.toString(),
      'timestamp': timestamp.toIso8601String(),
      'affectedArea': affectedArea,
    };
  }
}

/// Alert severity levels
enum AlertSeverity {
  info,
  warning,
  critical;

  static AlertSeverity fromString(String value) {
    switch (value.toLowerCase()) {
      case 'info':
        return AlertSeverity.info;
      case 'warning':
        return AlertSeverity.warning;
      case 'critical':
        return AlertSeverity.critical;
      default:
        return AlertSeverity.info;
    }
  }

  @override
  String toString() {
    switch (this) {
      case AlertSeverity.info:
        return 'info';
      case AlertSeverity.warning:
        return 'warning';
      case AlertSeverity.critical:
        return 'critical';
    }
  }
}
