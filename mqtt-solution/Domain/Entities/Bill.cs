using System;

namespace Domain.Entities
{
    public class Bill
    {
        // Unique ID of Bill
        public Guid Id { get; set; } = Guid.NewGuid();

        // Unique ID of Client this bill is for
        public Guid ClientId { get; set; }

        // Total kWh used (sum of all readings)
        public float TotalConsumption { get; set; }

        // Price per kWh at time of calculation (0.12 for now) 
        public decimal PricePerKwh { get; set; }

        // Final total cost for this bill
        public decimal TotalCost { get; set; }

        // Bill calculation timestamp
        public DateTime CalculatedAt { get; set; }

        // Billing period start and end timestamps
        public DateTime BillingPeriodStart { get; set; }
        public DateTime BillingPeriodEnd { get; set; }
    }
}
