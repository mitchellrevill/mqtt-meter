using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Mqtt.DatabaseContext;
using Infrastructure.Mqtt.Repositories;
using Infrastructure.Mqtt.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mqtt.IntegrationTests.Data
{
    public class BaseEfRepoTest
    {
        protected MqttDbContext _dbContext;

        protected BaseEfRepoTest()
        {
            var options = CreateNewContextOptions();
            _dbContext = new MqttDbContext(options);
        }

        protected static DbContextOptions<MqttDbContext> CreateNewContextOptions()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<MqttDbContext>();
            builder
                .UseInMemoryDatabase("TestDb")
                .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }
    }
}
