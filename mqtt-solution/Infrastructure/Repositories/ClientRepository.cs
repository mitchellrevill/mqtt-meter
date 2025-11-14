using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{

    /// Entity Framework Core repository for working with clients and their readings.

    public class ClientRepository : IClientRepository
    {
        private readonly MqttDbContext _context;


        /// Creates a new ClientRepository using the given database context.

        public ClientRepository(MqttDbContext context)
        {
            _context = context;
        }


        /// Gets all clients, including their readings, from the database.
        /// This is used by the billing service to calculate usage.

        public async Task<IEnumerable<Client>> GetAll()
        {
            // Include the Readings navigation property so each client
            // comes back with all of their meter readings.
            return await _context.Client
                .Include(c => c.Readings)
                .ToListAsync();
        }
    }
}
