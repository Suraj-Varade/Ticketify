using Core.Entities;
using Core.Interfaces;
using Core.RequestHelpers;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers;
public class TicketsController : BaseApiController
{
    private readonly ITicketRepository _ticketRepository;

    public TicketsController(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }
    [HttpGet]
    public async Task<ActionResult<PagedList<Ticket>>> GetTickets([FromQuery]TicketParams ticketParams)
    {
        Log.Information($"fetching all tickets");
        var pagedTickets = await _ticketRepository.GetTicketsAsync(ticketParams);
        //add pagination into the response
        Response.Headers.Append("X-Pagination",
            System.Text.Json.JsonSerializer.Serialize(new
            {
                pagedTickets.TotalCount,
                pagedTickets.PageSize,
                pagedTickets.PageNumber
            }));
        return Ok(pagedTickets);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ticket>> GetTicket(int id)
    {
        Log.Information($"fetching ticket with id : {id}");
        var ticket = await _ticketRepository.GetTicketByIdAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult> AddTicket(Ticket ticket)
    {
        Log.Information($"received new ticket creation request: {@ticket}", ticket);
        _ticketRepository.AddTicket(ticket);
        if (!await _ticketRepository.SaveChangesAsync())
        {
            return BadRequest("problem adding ticket");
        }
        return CreatedAtAction("GetTicket", new {id = ticket.Id}, ticket);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateTicket(int id, Ticket ticket)
    {
        Log.Information($"updating ticket of id : {id}");
        if (ticket.Id != id)
        {
            return BadRequest("cannot update this ticket");
        }
        if (!await _ticketRepository.TicketExists(id))
        {
            return NotFound();
        }
        _ticketRepository.UpdateTicket(ticket);
        if (!await _ticketRepository.SaveChangesAsync())
        {
            return BadRequest("problem updating ticket");
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteTicket(int id)
    {
        Log.Information($"deleting ticket of id : {id}");
        var ticket = await _ticketRepository.GetTicketByIdAsync(id);
        if (ticket == null)
        {
            return NotFound("ticket not found");
        }
        _ticketRepository.DeleteTicket(ticket);
        if (!await _ticketRepository.SaveChangesAsync())
        {
            return BadRequest("problem deleting ticket");
        }
        return NoContent();
    }
}