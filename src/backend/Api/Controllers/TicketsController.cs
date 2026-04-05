using InternalTicketManager.Application.Tickets;
using InternalTicketManager.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalTicketManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetTicketsAsync(CancellationToken cancellationToken)
    {
        var tickets = await _ticketService.GetTicketsAsync(cancellationToken);
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id, cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpGet("{ticketId:guid}/comments")]
    [ProducesResponseType<IReadOnlyList<CommentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetCommentsAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var comments = await _ticketService.GetCommentsAsync(ticketId, cancellationToken);
        return comments is null ? NotFound() : Ok(comments);
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Developer)}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        if (result.AssignedDevelopersInvalid)
        {
            ModelState.AddModelError(nameof(CreateTicketRequest.AssignedDeveloperIds), "Assigned developers must exist and have the Developer role.");
            return ValidationProblem(ModelState);
        }

        return Created($"/api/tickets/{result.Ticket!.Id}", result.Ticket);
    }

    [HttpPost("{ticketId:guid}/comments")]
    [ProducesResponseType<CommentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Developer)}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        if (result.AssignedDevelopersInvalid)
        {
            ModelState.AddModelError(nameof(UpdateTicketRequest.AssignedDeveloperIds), "Assigned developers must exist and have the Developer role.");
            return ValidationProblem(ModelState);
        }

        return Ok(result.Ticket);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _ticketService.DeleteTicketAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
