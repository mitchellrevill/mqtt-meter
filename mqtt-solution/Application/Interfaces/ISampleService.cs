using Domain.Entities.SampleEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISampleService
    {
        Task<IEnumerable<Sample>> GetAll();
    }
}
