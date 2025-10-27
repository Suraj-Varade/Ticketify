using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Test.Helpers;

// we will be using in-memory db for our tests.
public class TestDbContextFactory
{
    public static TicketContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TicketContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new TicketContext(options);
        context.Database.EnsureCreated();
        return context;
    }  
}