using Application.Interfaces.Repositories;
using Domain.Entities.SampleEntities;
using Infrastructure.Mqtt.DatabaseContext;
using Infrastructure.Mqtt.Services.Mocking;
using Infrastructure.Mqtt.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Infrastructure.Mqtt.Repositories
{
    public class ReadingRepository : IReadingRepository
    {
        private readonly MqttDbContext _context;

        public ReadingRepository(MqttDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reading>> GetAll()
        {
            return _context.Reading;
        }

        public async Task<Reading> AddAsync(Reading reading)
        {
            await _context.Reading.AddAsync(reading);
            await _context.SaveChangesAsync();
            return reading;
        }
    }
}
