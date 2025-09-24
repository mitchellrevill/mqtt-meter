using Application.Interfaces.Repositories;
using Infrastructure.DatabaseContext;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MqttDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemory");
            });

            services.AddScoped<ISampleRepository, SampleRepository>();
            services.AddScoped<IReadingRepository, ReadingRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();

            return services;
        }
    }
}
