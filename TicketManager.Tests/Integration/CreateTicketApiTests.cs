using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TicketManager.Application.Commands.CreateTicket;
using TicketManager.Application.DTOs;
using TicketManager.Domain.Enums;

namespace TicketManager.Tests.Integration;

public class CreateTicketApiTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public CreateTicketApiTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ValidCommand_PersistsTicketAndReturns201()
    {
        var command = new CreateTicketCommand("Écran cassé", "L'écran ne s'allume plus", Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/tickets", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TicketDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be(command.Title);
        created.Status.Should().Be(TicketStatus.Open);
        created.Category.Should().Be("General");

        var getResponse = await _client.GetAsync($"/api/tickets/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TicketDto>();
        fetched!.Id.Should().Be(created.Id);
        fetched.Title.Should().Be(command.Title);
    }

    [Fact]
    public async Task Create_EmptyTitle_Returns400()
    {
        var command = new CreateTicketCommand("", "Description", Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/tickets", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
