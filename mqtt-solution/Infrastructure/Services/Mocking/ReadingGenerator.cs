using System;
using Bogus;
using Domain.Entities;

namespace Infrastructure.Services.Mocking;

public class ReadingGenerator : Faker<Reading>
{
    public ReadingGenerator()
    {
        // Generate date between last 2 days to the last 12 hours
        RuleFor(reading => reading.TimeStamp, timeStamp => timeStamp.Date.Between(DateTime.Now.AddDays(-2), DateTime.Now.AddHours(-12)));
        RuleFor(reading => reading.Value, value => value.Random.Float(0, 10));
    }
}
