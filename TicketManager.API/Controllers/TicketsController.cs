using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketManager.API.Models;
using TicketManager.Application.Commands.CloseTicket;
using TicketManager.Application.Commands.CreateTicket;
using TicketManager.Application.Commands.DeleteTicket;
using TicketManager.Application.Commands.ResolveTicket;
using TicketManager.Application.Commands.StartTicketProgress;
using TicketManager.Application.Commands.UpdateTicket;
using TicketManager.Application.Queries.GetAllTickets;
using TicketManager.Application.Queries.GetTicketById;

namespace TicketManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTicketByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllTicketsQuery(), ct);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTicketRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateTicketCommand(id, request.Title, request.Description), ct);
        return Ok(result);
    }

    [HttpPost("{id}/start-progress")]
    public async Task<IActionResult> StartProgress(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new StartTicketProgressCommand(id), ct);
        return Ok(result);
    }

    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResolveTicketCommand(id), ct);
        return Ok(result);
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CloseTicketCommand(id), ct);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTicketCommand(id), ct);
        return NoContent();
    }
}
