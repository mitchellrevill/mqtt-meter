using Domain.Entities;
using Domain.Entities.SampleEntities;
using Infrastructure.DatabaseContext;
using Infrastructure.Services.Mocking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public static class GeneratorHelper
    {
        public static void AddEntities<TItem>(List<TItem> entities, MqttDbContext context)
        {
            entities.ForEach(entity =>
            {
                context.Add(entity);
            });

            context.SaveChanges();
        }

        public static void SeedData(MqttDbContext context)
        {
            var fakeClients = new ClientGenerator(context).Generate(50);

            GeneratorHelper.AddEntities<Client>(fakeClients, context);
        }
    }
}
