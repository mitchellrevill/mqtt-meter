using Bogus;
using Domain.Entities;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Mocking
{
    public class ClientGenerator : Faker<Client>
    {
        private readonly MqttDbContext _context;

        public ClientGenerator(MqttDbContext context)
        {
            _context = context;

            RuleFor(client => client.Readings, () =>
            {
                var fakeReadings = new ReadingGenerator().GenerateBetween(2, 20);
                GeneratorHelper.AddEntities<Reading>(fakeReadings, _context);

                return fakeReadings;
            });
        }
    }
}
