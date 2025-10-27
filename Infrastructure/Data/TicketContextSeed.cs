using System.Reflection;
using System.Text.Json;
using Core.Entities;

namespace Infrastructure.Data;

public class TicketContextSeed
{
    public static async Task SeedAsync(TicketContext context)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var filePath = Path.Combine(basePath, "Data", "SeedData", "Tickets.json");
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!context.Tickets.Any())
        {
            var ticketData = await File.ReadAllTextAsync(filePath);
            var tickets = JsonSerializer.Deserialize<List<Ticket>>(ticketData);
            if (tickets == null)
            {
                return;
            }
            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync();
        }
    }
}