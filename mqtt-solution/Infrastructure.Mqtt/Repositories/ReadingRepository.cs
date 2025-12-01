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

        public async Task<IEnumerable<Reading>> GetByUserId(string userId)
        {
            return _context.Reading.Where(r => r.UserId == userId);
        }

        public async Task<Reading> AddAsync(Reading reading)
        {
            await _context.Reading.AddAsync(reading);
            await _context.SaveChangesAsync();
            return reading;
        }

        public async Task DeleteByUserIdAsync(string userId)
        {
            var toRemove = _context.Reading.Where(r => r.UserId == userId).ToList();
            if (toRemove.Count == 0) return;

            _context.Reading.RemoveRange(toRemove);
            await _context.SaveChangesAsync();
        }

        public async Task InsertBatchAsync(List<Reading> readings)
        {
            readings.ForEach(async reading => {await _context.Reading.AddAsync(reading);});
            await _context.SaveChangesAsync();
        }
    }
}
