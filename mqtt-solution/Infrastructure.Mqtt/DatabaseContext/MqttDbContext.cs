using Domain.Entities;
using Domain.Entities.SampleEntities;
using Infrastructure.Mqtt.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mqtt.DatabaseContext
{
    public class MqttDbContext : DbContext
    {
        public MqttDbContext(DbContextOptions<MqttDbContext> options) : base(options)
        {
        }

        public DbSet<Sample> Sample { get; set; }
        public DbSet<Reading> Reading { get; set; }
        public DbSet<Client> Client { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sample>()
                .HasKey(a => new { a.Id });

            modelBuilder.Entity<Reading>()
                .HasKey(a => new { a.Id });
            
            modelBuilder.Entity<Client>()
                .HasKey(a => new { a.Id });
        }
    }
}
