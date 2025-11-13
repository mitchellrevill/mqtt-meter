class CreateReadingRequest {
  final String userId;
  final double value;

  const CreateReadingRequest({
    required this.userId,
    required this.value,
  });

  /// Convert to JSON for API request body
  Map<String, dynamic> toJson() {
    return {'userId': userId, 'value': value};
  }

  /// Factory constructor from JSON (if needed for testing/mocking)
  factory CreateReadingRequest.fromJson(Map<String, dynamic> json) {
    return CreateReadingRequest(
      userId: json['userId'] as String,
      value: (json['value'] as num).toDouble(),
    );
  }

  /// Create a copy with some fields replaced
  CreateReadingRequest copyWith({String? userId, double? value}) {
    return CreateReadingRequest(
      userId: userId ?? this.userId,
      value: value ?? this.value,
    );
  }

  @override
  String toString() {
    return 'CreateReadingRequest(userId: $userId, value: $value)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is CreateReadingRequest &&
        other.userId == userId &&
        other.value == value;
  }

  @override
  int get hashCode => Object.hash(userId, value);
}
