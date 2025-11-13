using Application.Interfaces.Repositories;
using Domain.Entities.SampleEntities;
using Infrastructure.DatabaseContext;
using Infrastructure.Services.Mocking;
using Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Infrastructure.Repositories
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
