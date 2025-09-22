using Domain.Entities;
using Domain.Entities.SampleEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IReadingRepository
    {
        public Task<IEnumerable<Reading>> GetAll();
    }
}
