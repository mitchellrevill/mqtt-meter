using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IClientRepository
    // Gets all clients from the database with their readings
    {
        public Task<IEnumerable<Client>> GetAll();
    }
}
