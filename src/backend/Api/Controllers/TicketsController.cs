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

    [HttpGet("{ticketId:guid}/comments")]
    [ProducesResponseType<IReadOnlyList<CommentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetCommentsAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var comments = await _ticketService.GetCommentsAsync(ticketId, cancellationToken);
        return comments is null ? NotFound() : Ok(comments);
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

    [HttpPost("{ticketId:guid}/comments")]
    [ProducesResponseType<CommentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentResponse>> CreateCommentAsync(
        Guid ticketId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.CreateCommentAsync(ticketId, request, cancellationToken);
        if (result.TicketNotFound)
        {
            return NotFound();
        }

        return Created($"/api/tickets/{ticketId}/comments/{result.Comment!.Id}", result.Comment);
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
