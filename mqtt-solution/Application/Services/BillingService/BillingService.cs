using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Services.BillingService
{
    /// Implements the billing logic for clients by using their meter readings.
    public class BillingService : IBillingService
    {
        private readonly IBillingRepository _billingRepository;
        private readonly IClientRepository _clientRepository;

        // 0.12 for now, will switch to dynamic pricing later.

        private const decimal PricePerKwh = 0.12m;

        public BillingService(
            IBillingRepository billingRepository,
            IClientRepository clientRepository)
        {
            _billingRepository = billingRepository;
            _clientRepository = clientRepository;
        }

        /// Calculates a new bill for the specified client, saves it and returns the generated bill
        public async Task<Bill> CalculateBill(Guid clientId)
        {
            var clients = await _clientRepository.GetAll();
            var client = clients.FirstOrDefault(c => c.Id == clientId);

            if (client == null)
            {
                throw new Exception($"Client with ID {clientId} not found");
            }

            float totalConsumption = 0;
            DateTime? earliestReading = null;
            DateTime? latestReading = null;

            // check if client even has any readings
            if (client.Readings != null && client.Readings.Any())
            {
                totalConsumption = client.Readings.Sum(r => r.Value);
                earliestReading = client.Readings.Min(r => r.TimeStamp);
                latestReading = client.Readings.Max(r => r.TimeStamp);
            }

            //Calculate the total cost
            var totalCost = (decimal)totalConsumption * PricePerKwh;
            var now = DateTime.UtcNow;

            //Create the bill object
            var bill = new Bill
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                TotalConsumption = totalConsumption,
                PricePerKwh = PricePerKwh,
                TotalCost = totalCost,
                CalculatedAt = now,
                BillingPeriodStart = earliestReading ?? now,
                BillingPeriodEnd = latestReading ?? now
            };

            // Step 5: Save the bill
            await _billingRepository.AddBill(bill);

            // Step 6: Return the saved bill
            return bill;
        }


        /// Returns the most recent bill for a client, if one exists.
        public Task<Bill?> GetBillForClient(Guid clientId)
        {
            return _billingRepository.GetBillByClientId(clientId);
        }

        /// Returns all bills in the system (useful for testing).
        public Task<IEnumerable<Bill>> GetAllBills()
        {
            return _billingRepository.GetAllBills();
        }
    }
}
