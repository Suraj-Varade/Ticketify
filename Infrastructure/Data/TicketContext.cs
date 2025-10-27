using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class TicketContext: DbContext
{
    public TicketContext(DbContextOptions<TicketContext> options) : base(options)
    {
    }
    public DbSet<Ticket> Tickets { get; set; }
}