class CreateReadingRequest {
  final String userId;
  final double kwhSinceLast;

  const CreateReadingRequest({
    required this.userId,
    required this.kwhSinceLast,
  });

  /// Convert to JSON for API request body
  Map<String, dynamic> toJson() {
    return {'userId': userId, 'kwhSinceLast': kwhSinceLast};
  }

  /// Factory constructor from JSON (if needed for testing/mocking)
  factory CreateReadingRequest.fromJson(Map<String, dynamic> json) {
    return CreateReadingRequest(
      userId: json['userId'] as String,
      kwhSinceLast: (json['kwhSinceLast'] as num).toDouble(),
    );
  }

  /// Create a copy with some fields replaced
  CreateReadingRequest copyWith({String? userId, double? kwhSinceLast}) {
    return CreateReadingRequest(
      userId: userId ?? this.userId,
      kwhSinceLast: kwhSinceLast ?? this.kwhSinceLast,
    );
  }

  @override
  String toString() {
    return 'CreateReadingRequest(userId: $userId, kwhSinceLast: $kwhSinceLast)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is CreateReadingRequest &&
        other.userId == userId &&
        other.kwhSinceLast == kwhSinceLast;
  }

  @override
  int get hashCode => Object.hash(userId, kwhSinceLast);
}
