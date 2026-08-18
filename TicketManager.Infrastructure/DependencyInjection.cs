using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Interfaces;
using TicketManager.Infrastructure.AI;
using TicketManager.Infrastructure.Persistence;
using TicketManager.Infrastructure.Repositories;

namespace TicketManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<ITicketRepository, TicketRepository>();

        services.AddHttpClient<ITicketAIClassifier, HttpTicketAIClassifier>(client =>
        {
            client.BaseAddress = new Uri(configuration["AiService:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
