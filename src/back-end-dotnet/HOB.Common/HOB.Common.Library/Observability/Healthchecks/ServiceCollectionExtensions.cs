using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace HOB.Common.Library.Observability.HealthChecks;

public static class ServiceCollectionExtensions
{
    public static void AddServiceHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlConnection = configuration.GetConnectionString("DefaultConnection");
        var rabbitMqConnection = configuration.GetConnectionString("RabbitMQ");

        services.AddHealthChecks()
            .AddSqlServer(
                connectionString: sqlConnection!,
                name: "sqlserver",
                tags: new[] { "db", "sql", "sqlserver" })
            .AddRabbitMQ(
                sp =>
                {
                    var factory = new ConnectionFactory();
                    factory.Uri = new Uri(rabbitMqConnection!);
                    return factory.CreateConnection();
                },
                name: "rabbitmq",
                tags: new[] { "messagebus", "rabbitmq" });
    }
}