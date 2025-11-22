using Application.Interfaces.Repositories;
using Domain.Entities.SampleEntities;
using Infrastructure.Mqtt.DatabaseContext;
using Infrastructure.Mqtt.Services;
using Infrastructure.Mqtt.Services.Mocking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infrastructure.Mqtt.Repositories
{
    public class SampleRepository : ISampleRepository
    {
        private readonly MqttDbContext _context;

        public SampleRepository(MqttDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sample>> GetAll()
        {
            return _context.Sample;
        }
    }
}
