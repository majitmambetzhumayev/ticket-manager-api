using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Enums;

namespace TicketManager.Infrastructure.AI;

public class HttpTicketAIClassifier : ITicketAIClassifier
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpTicketAIClassifier> _logger;

    public HttpTicketAIClassifier(HttpClient httpClient, ILogger<HttpTicketAIClassifier> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TicketClassificationResult> ClassifyAsync(string title, string description, CancellationToken ct = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/classify",
                new ClassifyRequest(title, description),
                JsonOptions,
                ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ai-service returned {StatusCode} for ticket {Title}", response.StatusCode, title);
                return TicketClassificationResult.Default;
            }

            var payload = await response.Content.ReadFromJsonAsync<ClassifyResponse>(JsonOptions, ct);
            if (payload is null || !Enum.TryParse<TicketPriority>(payload.Priority, ignoreCase: true, out var priority))
            {
                _logger.LogWarning("ai-service returned an unparsable classification for ticket {Title}", title);
                return TicketClassificationResult.Default;
            }

            return new TicketClassificationResult(priority, payload.Category, payload.SuggestedResponse, payload.GroundedInHistory);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ai-service classification failed for ticket {Title}, falling back to default", title);
            return TicketClassificationResult.Default;
        }
    }

    private record ClassifyRequest(string Title, string Description);

    private record ClassifyResponse(string Priority, string Category, string? SuggestedResponse, bool GroundedInHistory);
}
