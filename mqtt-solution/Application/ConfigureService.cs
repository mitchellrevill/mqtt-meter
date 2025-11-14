using Application.Behavior;
using Application.Behaviors;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Services.BillingService;
using Application.Services.ClientService;
using Application.Services.ReadingService;
using Application.Services.SampleService;
using MediatR;
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
            services.AddScoped<IBillingService, BillingService>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
                cfg.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
            });

            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));

            return services;
        }
    }
}
