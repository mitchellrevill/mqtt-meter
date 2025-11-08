class BillingModel {
  final String userId;
  final DateTime billingPeriodStart;
  final DateTime billingPeriodEnd;
  final double totalKwhUsed;
  final double ratePerKwh;
  final double totalAmount;
  final DateTime lastUpdated;

  const BillingModel({
    required this.userId,
    required this.billingPeriodStart,
    required this.billingPeriodEnd,
    required this.totalKwhUsed,
    required this.ratePerKwh,
    required this.totalAmount,
    required this.lastUpdated,
  });

  /// Factory constructor to create BillingModel from JSON response
  factory BillingModel.fromJson(Map<String, dynamic> json) {
    return BillingModel(
      userId: json['userId'] as String? ?? 'unknown',
      billingPeriodStart: json['billingPeriodStart'] != null 
          ? DateTime.parse(json['billingPeriodStart'] as String)
          : DateTime.now().subtract(const Duration(days: 30)),
      billingPeriodEnd: json['billingPeriodEnd'] != null
          ? DateTime.parse(json['billingPeriodEnd'] as String)
          : DateTime.now(),
      totalKwhUsed: (json['totalKwhUsed'] as num?)?.toDouble() ?? 0.0,
      ratePerKwh: (json['ratePerKwh'] as num?)?.toDouble() ?? 0.15,
      totalAmount: (json['totalAmount'] as num?)?.toDouble() ?? 0.0,
      lastUpdated: json['lastUpdated'] != null
          ? DateTime.parse(json['lastUpdated'] as String)
          : DateTime.now(),
    );
  }

  /// Convert BillingModel to JSON
  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
      'billingPeriodStart': billingPeriodStart.toIso8601String(),
      'billingPeriodEnd': billingPeriodEnd.toIso8601String(),
      'totalKwhUsed': totalKwhUsed,
      'ratePerKwh': ratePerKwh,
      'totalAmount': totalAmount,
      'lastUpdated': lastUpdated.toIso8601String(),
    };
  }

  /// Create a copy with some fields replaced
  BillingModel copyWith({
    String? userId,
    DateTime? billingPeriodStart,
    DateTime? billingPeriodEnd,
    double? totalKwhUsed,
    double? ratePerKwh,
    double? totalAmount,
    DateTime? lastUpdated,
  }) {
    return BillingModel(
      userId: userId ?? this.userId,
      billingPeriodStart: billingPeriodStart ?? this.billingPeriodStart,
      billingPeriodEnd: billingPeriodEnd ?? this.billingPeriodEnd,
      totalKwhUsed: totalKwhUsed ?? this.totalKwhUsed,
      ratePerKwh: ratePerKwh ?? this.ratePerKwh,
      totalAmount: totalAmount ?? this.totalAmount,
      lastUpdated: lastUpdated ?? this.lastUpdated,
    );
  }

  @override
  String toString() {
    return 'BillingModel(userId: $userId, totalKwhUsed: $totalKwhUsed, totalAmount: \$${totalAmount.toStringAsFixed(2)})';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is BillingModel &&
        other.userId == userId &&
        other.billingPeriodStart == billingPeriodStart &&
        other.billingPeriodEnd == billingPeriodEnd &&
        other.totalKwhUsed == totalKwhUsed &&
        other.ratePerKwh == ratePerKwh &&
        other.totalAmount == totalAmount &&
        other.lastUpdated == lastUpdated;
  }

  @override
  int get hashCode => Object.hash(
    userId,
    billingPeriodStart,
    billingPeriodEnd,
    totalKwhUsed,
    ratePerKwh,
    totalAmount,
    lastUpdated,
  );
}
