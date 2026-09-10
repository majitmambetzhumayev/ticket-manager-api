namespace TicketManager.API;

/// <summary>
/// Gates ticket mutations (POST/PUT/DELETE) behind Azure Container Apps' Easy
/// Auth: authenticated requests carry an X-MS-CLIENT-PRINCIPAL-ID header
/// injected by the platform, which can't be forged by an external caller.
/// Reads stay public (GET) - that's what ai-service's own server-to-server
/// callback uses, and it can't do an interactive GitHub login.
/// Gates by verb only, not by route: fine while /api/tickets is the only
/// resource in the app, but a future non-ticket POST/PUT/DELETE would
/// silently inherit this too - revisit if that happens.
/// </summary>
public class RequireAuthForMutationsMiddleware
{
    public const string ClientPrincipalIdHeader = "X-MS-CLIENT-PRINCIPAL-ID";

    private static bool IsProtectedMethod(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method);

    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public RequireAuthForMutationsMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Easy Auth only exists on Azure. Enforcing this locally would block
        // every ticket creation in dev, where there's no login to perform.
        if (_env.IsDevelopment() || !IsProtectedMethod(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.ContainsKey(ClientPrincipalIdHeader))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context);
    }
}

public static class RequireAuthForMutationsMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthForMutations(this IApplicationBuilder app) =>
        app.UseMiddleware<RequireAuthForMutationsMiddleware>();
}
