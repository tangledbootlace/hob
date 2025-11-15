using HOB.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HobDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }

    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = configuration.GetConnectionString("RabbitMQ");
                cfg.Host(rabbitMqConfig);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
