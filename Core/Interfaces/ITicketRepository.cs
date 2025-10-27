using Core.Entities;
using Core.RequestHelpers;

namespace Core.Interfaces;

public interface ITicketRepository
{
    //GET
    Task<PagedList<Ticket>> GetTicketsAsync(TicketParams ticketParams);
    Task<Ticket?> GetTicketByIdAsync(int id);
    
    //POST
    void AddTicket(Ticket ticket);
    
    //PUT
    void UpdateTicket(Ticket ticket);
    
    //DELETE
    void DeleteTicket(Ticket ticket);
    
    Task<bool> TicketExists(int id);
    
    // Save Changes
    Task<bool> SaveChangesAsync();
}