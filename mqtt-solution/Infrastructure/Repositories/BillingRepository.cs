using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{

    /// Entity Framework Core repository for storing and retrieving bills.

    public class BillingRepository : IBillingRepository
    {
        private readonly MqttDbContext _context;


        /// Creates a new BillingRepository using the given database context.

        public BillingRepository(MqttDbContext context)
        {
            _context = context;
        }


        /// Adds a new bill to the database and saves the changes.

        public async Task<Bill> AddBill(Bill bill)
        {
            // Track the new bill entity
            await _context.Bill.AddAsync(bill);

            // Persist changes to the database
            await _context.SaveChangesAsync();

            // Return the saved bill (including its ID)
            return bill;
        }
        /// Gets the most recent bill for a specific client, if one exists.

        public async Task<Bill?> GetBillByClientId(Guid clientId)
        {
            // Filter bills by client, order by calculation time (newest first),
            // and return the most recent one or null if none exist.
            return await _context.Bill
                .Where(b => b.ClientId == clientId)
                .OrderByDescending(b => b.CalculatedAt) // Get the most recent bill
                .FirstOrDefaultAsync();
        }

        /// Gets all bills in the system.

        public async Task<IEnumerable<Bill>> GetAllBills()
        {
            // Return all bills as a list
            return await _context.Bill.ToListAsync();
        }
    }
}
