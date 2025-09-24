using System;
using Bogus;
using Domain.Entities.SampleEntities;

namespace Infrastructure.Services.Mocking;

public class SampleGenerator : Faker<Sample>
{
    public SampleGenerator()
    {
        RuleFor(sample => sample.Id, id => id.Random.Number(0, 1000));
        RuleFor(sample => sample.SampleData, sampleData => sampleData.Random.Number(0, 10));
    }
}
