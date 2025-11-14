using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBillingService
    {
        // Calculate and save a bill for a specific client
        Task<Bill> CalculateBill(Guid clientId);

        // Get an existing bill for a client
        Task<Bill?> GetBillForClient(Guid clientId);

        // Get all bills
        Task<IEnumerable<Bill>> GetAllBills();
    }
}
