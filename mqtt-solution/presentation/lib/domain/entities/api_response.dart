class ApiResponse<T> {
  final bool success;
  final String message;
  final T? data;
  final String? error;

  const ApiResponse({
    required this.success,
    required this.message,
    this.data,
    this.error,
  });

  /// Factory constructor for successful responses
  factory ApiResponse.success({required String message, T? data}) {
    return ApiResponse<T>(success: true, message: message, data: data);
  }

  /// Factory constructor for error responses
  factory ApiResponse.error({required String message, String? error}) {
    return ApiResponse<T>(success: false, message: message, error: error);
  }

  /// Create from JSON response
  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>)? fromJsonT,
  ) {
    return ApiResponse<T>(
      success: json['success'] as bool? ?? false,
      message: json['message'] as String? ?? '',
      data:
          json['data'] != null && fromJsonT != null
              ? fromJsonT(json['data'] as Map<String, dynamic>)
              : null,
      error: json['error'] as String?,
    );
  }

  /// Convert to JSON
  Map<String, dynamic> toJson() {
    return {
      'success': success,
      'message': message,
      if (data != null) 'data': data,
      if (error != null) 'error': error,
    };
  }

  @override
  String toString() {
    return 'ApiResponse(success: $success, message: $message, data: $data, error: $error)';
  }
}
