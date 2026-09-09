using StackExchange.Redis;

namespace TicketManager.API;

public static class RateLimitingExtensions
{
    public static async Task<IServiceCollection> AddApiRateLimitingAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        var options = ConfigurationOptions.Parse(connectionString);
        // Let the app start even if Redis isn't reachable yet (container
        // startup ordering isn't guaranteed); the multiplexer keeps retrying
        // in the background, and the middleware fails open on top of that.
        options.AbortOnConnectFail = false;

        // Connected eagerly here (awaited, at startup) rather than lazily via
        // the DI factory, so the blocking connect attempt happens once during
        // startup instead of stalling whichever request first resolves it.
        var multiplexer = await ConnectionMultiplexer.ConnectAsync(options);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);

        return services;
    }
}
