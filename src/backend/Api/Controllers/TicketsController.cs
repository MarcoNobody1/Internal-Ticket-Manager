using InternalTicketManager.Application.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace InternalTicketManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetTicketsAsync(CancellationToken cancellationToken)
    {
        var tickets = await _ticketService.GetTicketsAsync(cancellationToken);
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id, cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketResponse>> CreateTicketAsync(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.CreateTicketAsync(request, cancellationToken);
        if (result.ProjectNotFound)
        {
            ModelState.AddModelError(nameof(CreateTicketRequest.ProjectId), "Project does not exist.");
            return ValidationProblem(ModelState);
        }

        return Created($"/api/tickets/{result.Ticket!.Id}", result.Ticket);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> UpdateTicketAsync(
        Guid id,
        [FromBody] UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.UpdateTicketAsync(id, request, cancellationToken);
        if (result.TicketNotFound)
        {
            return NotFound();
        }

        if (result.ProjectNotFound)
        {
            ModelState.AddModelError(nameof(UpdateTicketRequest.ProjectId), "Project does not exist.");
            return ValidationProblem(ModelState);
        }

        return Ok(result.Ticket);
    }
}
