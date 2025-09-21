using Domain.Entities.SampleEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DatabaseContext
{
    public class MqttDbContext : DbContext
    {
        public MqttDbContext(DbContextOptions<MqttDbContext> options) : base(options)
        {
        }

        public DbSet<Sample> Sample { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sample>()
                .HasKey(a => new { a.Id });
        }
    }
}
