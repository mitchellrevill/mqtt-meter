class ReadingModel {
  final String id;
  final DateTime timeStamp;
  final double value;

  const ReadingModel({
    required this.id,
    required this.timeStamp,
    required this.value,
  });

  /// Factory constructor to create ReadingModel from JSON response
  factory ReadingModel.fromJson(Map<String, dynamic> json) {
    return ReadingModel(
      id: json['id'] as String,
      timeStamp: DateTime.parse(json['timeStamp'] as String),
      value: (json['value'] as num).toDouble(),
    );
  }

  /// Convert ReadingModel to JSON for API requests
  Map<String, dynamic> toJson() {
    return {'id': id, 'timeStamp': timeStamp.toIso8601String(), 'value': value};
  }

  /// Create a copy of this ReadingModel with some fields replaced
  ReadingModel copyWith({String? id, DateTime? timeStamp, double? value}) {
    return ReadingModel(
      id: id ?? this.id,
      timeStamp: timeStamp ?? this.timeStamp,
      value: value ?? this.value,
    );
  }

  @override
  String toString() {
    return 'ReadingModel(id: $id, timeStamp: $timeStamp, value: $value)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is ReadingModel &&
        other.id == id &&
        other.timeStamp == timeStamp &&
        other.value == value;
  }

  @override
  int get hashCode => Object.hash(id, timeStamp, value);
}
