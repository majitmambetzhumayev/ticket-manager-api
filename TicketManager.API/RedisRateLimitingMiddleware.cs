using StackExchange.Redis;

namespace TicketManager.API;

/// <summary>
/// Per-IP fixed-window rate limiting backed by Redis, so the limit holds
/// across replicas (multiple pods/instances each had their own independent
/// in-memory counter before this, making the real limit N times the
/// configured one once the app scaled past a single instance).
/// </summary>
public class RedisRateLimitingMiddleware
{
    private const int PermitLimit = 100;
    private const int WindowSeconds = 60;

    // INCR then EXPIRE-only-on-first-hit, as one atomic script. Without the
    // script, two concurrent requests could both read the pre-increment
    // count and both decide they're under the limit, letting a client
    // briefly exceed it under contention.
    private const string FixedWindowScript = """
        local key = KEYS[1]
        local limit = tonumber(ARGV[1])
        local window = tonumber(ARGV[2])

        local count = redis.call('INCR', key)
        if count == 1 then
            redis.call('EXPIRE', key, window)
        end

        if count > limit then
            return 0
        end
        return 1
        """;

    private readonly RequestDelegate _next;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimitingMiddleware> _logger;

    public RedisRateLimitingMiddleware(RequestDelegate next, IConnectionMultiplexer redis, ILogger<RedisRateLimitingMiddleware> logger)
    {
        _next = next;
        _redis = redis;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (InfraEndpoints.All.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            await _next(context);
            return;
        }

        if (await IsAllowedAsync(context))
        {
            await _next(context);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        }
    }

    private async Task<bool> IsAllowedAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        try
        {
            var db = _redis.GetDatabase();
            var result = await db.ScriptEvaluateAsync(
                FixedWindowScript,
                [(RedisKey)$"ratelimit:{clientIp}"],
                [PermitLimit, WindowSeconds]);

            return (int)result == 1;
        }
        catch (RedisException ex)
        {
            // Fail open: a broken rate limiter shouldn't take the whole API
            // down. Same resilience stance as HttpTicketAIClassifier. Catches
            // the RedisException base, not just RedisConnectionException, so
            // timeouts and server errors fail open too, not just outages.
            _logger.LogWarning(ex, "Redis unavailable, allowing request through unthrottled for {ClientIp}", clientIp);
            return true;
        }
    }
}

public static class RedisRateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRedisRateLimiting(this IApplicationBuilder app) =>
        app.UseMiddleware<RedisRateLimitingMiddleware>();
}
