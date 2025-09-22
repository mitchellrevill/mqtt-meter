using System;
using Bogus;
using Domain.Entities;

namespace Infrastructure.Services.Mocking;

public class ReadingGenerator : Faker<Reading>
{
    public ReadingGenerator()
    {
        RuleFor(reading => reading.TimeStamp, timeStamp => timeStamp.Date.Recent());
        RuleFor(reading => reading.Value, value => value.Random.Float(0, 10));
    }
}
