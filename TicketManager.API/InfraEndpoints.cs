namespace TicketManager.API;

internal static class InfraEndpoints
{
    public const string Health = "/health";
    public const string Metrics = "/metrics";

    public static readonly string[] All = [Health, Metrics];
}
