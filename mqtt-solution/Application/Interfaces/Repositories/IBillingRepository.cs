using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IBillingRepository
    {
        // Save a new bill to the database
        Task<Bill> AddBill(Bill bill);

        // Get a bill for a specific client
        Task<Bill?> GetBillByClientId(Guid clientId);

        // Get all bills (useful for admin/debugging)
        Task<IEnumerable<Bill>> GetAllBills();
    }
}
