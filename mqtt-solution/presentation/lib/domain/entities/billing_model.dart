class BillingModel {
  final String userId;
  final DateTime billingPeriodStart;
  final DateTime billingPeriodEnd;
  final double totalKwhUsed;
  final double ratePerKwh;
  final double totalAmount;
  final DateTime lastUpdated;
  final int readingCount;

  const BillingModel({
    required this.userId,
    required this.billingPeriodStart,
    required this.billingPeriodEnd,
    required this.totalKwhUsed,
    required this.ratePerKwh,
    required this.totalAmount,
    required this.lastUpdated,
    required this.readingCount,
  });

  /// Factory constructor to create BillingModel from JSON response
  factory BillingModel.fromJson(Map<String, dynamic> json) {
    final dynamic billingPeriodStartRaw = json['billingPeriodStart'] ?? json['BillingPeriodStart'];
    final dynamic billingPeriodEndRaw = json['billingPeriodEnd'] ?? json['BillingPeriodEnd'];
    final dynamic lastUpdatedRaw = json['lastUpdated'] ?? json['LastUpdated'];

    return BillingModel(
      userId: json['userId'] as String? ?? json['UserId'] as String? ?? 'unknown',
      billingPeriodStart: billingPeriodStartRaw != null
        ? DateTime.parse(billingPeriodStartRaw as String)
        : DateTime.now().subtract(const Duration(days: 30)),
      billingPeriodEnd: billingPeriodEndRaw != null
        ? DateTime.parse(billingPeriodEndRaw as String)
        : DateTime.now(),
      totalKwhUsed: (json['totalKwhUsed'] as num?)?.toDouble() ??
        (json['TotalKwhUsed'] as num?)?.toDouble() ??
        0.0,
      ratePerKwh: (json['ratePerKwh'] as num?)?.toDouble() ??
        (json['RatePerKwh'] as num?)?.toDouble() ??
        0.15,
      totalAmount: (json['totalAmount'] as num?)?.toDouble() ??
        (json['TotalAmount'] as num?)?.toDouble() ??
        0.0,
      lastUpdated: lastUpdatedRaw != null
        ? DateTime.parse(lastUpdatedRaw as String)
        : DateTime.now(),
      readingCount: json['readingCount'] as int? ?? json['ReadingCount'] as int? ?? 0,
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
      'readingCount': readingCount,
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
    int? readingCount,
  }) {
    return BillingModel(
      userId: userId ?? this.userId,
      billingPeriodStart: billingPeriodStart ?? this.billingPeriodStart,
      billingPeriodEnd: billingPeriodEnd ?? this.billingPeriodEnd,
      totalKwhUsed: totalKwhUsed ?? this.totalKwhUsed,
      ratePerKwh: ratePerKwh ?? this.ratePerKwh,
      totalAmount: totalAmount ?? this.totalAmount,
      lastUpdated: lastUpdated ?? this.lastUpdated,
      readingCount: readingCount ?? this.readingCount,
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
        other.lastUpdated == lastUpdated &&
        other.readingCount == readingCount;
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
    readingCount,
  );
}
