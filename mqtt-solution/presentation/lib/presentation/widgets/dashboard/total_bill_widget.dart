import 'package:flutter/material.dart';

class TotalBillWidget extends StatelessWidget {
  final double? totalAmount;

  const TotalBillWidget({super.key, this.totalAmount});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [Colors.deepPurple.shade700, Colors.deepPurple.shade900],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.deepPurple.withValues(alpha: 0.3),
            blurRadius: 8,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.end,
        children: [
          Text(
            'Total Bill',
            style: TextStyle(
              color: Colors.grey.shade300,
              fontSize: 14,
              fontWeight: FontWeight.w500,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            totalAmount != null ? '\$${totalAmount!.toStringAsFixed(2)}' : '--',
            style: TextStyle(
              color: totalAmount != null ? Colors.white : Colors.grey.shade400,
              fontSize: 24,
              fontWeight: FontWeight.bold,
            ),
          ),
          if (totalAmount == null)
            Text(
              'No data available',
              style: TextStyle(
                color: Colors.grey.shade500,
                fontSize: 12,
                fontStyle: FontStyle.italic,
              ),
            ),
        ],
      ),
    );
  }
}
