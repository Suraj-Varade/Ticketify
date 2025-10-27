using Core.Entities;
using Core.Interfaces;
using Core.RequestHelpers;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly TicketContext _context;

    public TicketRepository(TicketContext context)
    {
        _context = context;
    }

    public async Task<PagedList<Ticket>> GetTicketsAsync(TicketParams ticketParams)
    {
        var query = _context.Tickets.AsQueryable();
        if (!string.IsNullOrWhiteSpace(ticketParams.SearchTerm))
        {
            query = query
                .Where(t => t.Title.ToLower().Contains(ticketParams.SearchTerm.ToLower()) 
                            || t.Description.ToLower().Contains(ticketParams.SearchTerm.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(ticketParams.Status))
        {
            query = query
                .Where(t => t.Status == ticketParams.Status);
        }

        if (ticketParams.CreatedBy.HasValue)
        {
            query = query.Where(t => t.CreatedBy == ticketParams.CreatedBy.Value);
        }
        if (ticketParams.AssignTo.HasValue)
        {
            query = query.Where(t => t.AssignTo == ticketParams.AssignTo.Value);
        }
        
        //Sorting
        query = ticketParams.OrderBy.ToLower() switch
        {
            "title" => query.OrderBy(t => t.Title),
            "titledesc" => query.OrderByDescending(t => t.Title),
            "status" => query.OrderBy(t => t.Status),
            "statusdesc" => query.OrderByDescending(t => t.Status),
            "createdat" => query.OrderBy(t => t.CreatedAt),
            "createdatdesc" => query.OrderByDescending(t => t.CreatedAt),
            "modifiedat" => query.OrderBy(t => t.ModifiedAt),
            "modifiedatdesc" => query.OrderByDescending(t => t.ModifiedAt),
            _ => query.OrderByDescending(t => t.CreatedAt) // default
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((ticketParams.PageNumber - 1) * ticketParams.PageSize)
            .Take(ticketParams.PageSize)
            .ToListAsync();

        return new PagedList<Ticket>(items, totalCount, ticketParams.PageNumber, ticketParams.PageSize);
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        return ticket;
    }

    public void AddTicket(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
    }

    public void UpdateTicket(Ticket ticket)
    {
        _context.Entry(ticket).State = EntityState.Modified;
    }

    public void DeleteTicket(Ticket ticket)
    {
        _context.Tickets.Remove(ticket);
    }

    public async Task<bool> TicketExists(int id)
    {
        return await _context.Tickets.AnyAsync(t => t.Id == id);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}