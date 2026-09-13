using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Interfaces;
using TicketManager.Infrastructure.AI;
using TicketManager.Infrastructure.Persistence;
using TicketManager.Infrastructure.Repositories;
using TicketManager.Infrastructure.Search;

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
            // ai-service scales to zero in Azure: a cold start (Python boot +
            // the classify graph's OpenAI calls) can exceed the 15s this used
            // to be, so ticket creation would silently fall back to the
            // default classification on every first request after idle.
            client.Timeout = TimeSpan.FromSeconds(45);
        });

        var elasticsearchUri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
        var elasticsearchSettings = new ElasticsearchClientSettings(new Uri(elasticsearchUri))
            .RequestTimeout(TimeSpan.FromSeconds(5));
        services.AddSingleton(new ElasticsearchClient(elasticsearchSettings));
        services.AddScoped<ITicketSearchIndex, ElasticsearchTicketSearchService>();

        return services;
    }
}
