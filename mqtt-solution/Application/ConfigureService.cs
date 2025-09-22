using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Services.ClientService;
using Application.Services.ReadingService;
using Application.Services.SampleService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class ConfigureService
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ISampleService, SampleService>();
            services.AddScoped<IReadingService, ReadingService>();
            services.AddScoped<IClientService, ClientService>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }
    }
}
