using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using TicketManager.Application.Interfaces;
using TicketManager.Infrastructure.AI;
using TicketManager.Infrastructure.Persistence;

namespace TicketManager.Tests.Integration;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16").Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _dbContainer.GetConnectionString(),
                ["Redis:ConnectionString"] = _redisContainer.GetConnectionString()
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITicketAIClassifier>();
            services.AddScoped<ITicketAIClassifier, StubTicketAIClassifier>();

            // Elasticsearch is an external HTTP dependency, out of scope for a
            // DB integration test (same reasoning as the AI classifier swap
            // above) - without this, every write falls through the fail-open
            // catch on a real connection timeout, not a fast no-op.
            services.RemoveAll<ITicketSearchIndex>();
            var searchIndex = Substitute.For<ITicketSearchIndex>();
            searchIndex.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Guid>>([]));
            services.AddSingleton(searchIndex);
        });
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_dbContainer.StartAsync(), _redisContainer.StartAsync());

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(_dbContainer.DisposeAsync().AsTask(), _redisContainer.DisposeAsync().AsTask());
        await base.DisposeAsync();
    }
}
