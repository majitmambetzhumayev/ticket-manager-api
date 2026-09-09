using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using TicketManager.API;
using TicketManager.API.Middleware;
using TicketManager.Application.Behaviors;
using TicketManager.Application.Commands.CreateTicket;
using TicketManager.Infrastructure;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateTicketCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateTicketCommand>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddObservability();
await builder.Services.AddApiRateLimitingAsync(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Without this, RateLimitingExtensions' per-IP partitioning would see the
// ingress's IP for every client and rate-limit the whole app as one caller.
// KnownIPNetworks/KnownProxies are cleared to trust X-Forwarded-For from
// whoever reaches the container directly, on the assumption that Azure
// Container Apps' ingress is the only path in. That assumption isn't
// enforced by this code: if it ever stops holding (a VNet, a second proxy),
// a caller could forge the header and dodge the rate limit. Accepted here
// because this is a demo project with no auth and no sensitive data, not a
// production trust boundary. Azure Container Apps doesn't publish a small,
// stable IP range for its ingress that could be pinned instead.
var forwardedHeadersOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor };
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseRedisRateLimiting();

app.UseAuthorization();

app.MapControllers();
app.MapPrometheusScrapingEndpoint(InfraEndpoints.Metrics);
app.MapHealthChecks(InfraEndpoints.Health);

app.Run();

public partial class Program { }
