using System.Text.Json;
using Core.Entities;
using Core.RequestHelpers;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test;

public class TicketRepositoryTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public TicketRepositoryTests(ITestOutputHelper _testOutputHelper)
    {
        testOutputHelper = _testOutputHelper;
    }

    //create context
    private TicketContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TicketContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TicketContext(options);
    }
    
    #region GET
    [Trait("Category", "GET")]
    [Fact]
    public async Task GetTickets_ShouldReturnAllMatchingTickets()
    {
        var context = CreateContext();
        var repo = new TicketRepository(context);
        //add tickets
        foreach (var ticket in TicketDataHelper.TicketData())
        {
            repo.AddTicket(ticket);
        }

        await context.SaveChangesAsync();

        // verify
        var all_tickets = await repo.GetTicketsAsync(new TicketParams()
        {
            PageNumber = 1,
            PageSize = 2,
            CreatedBy = 1,
            OrderBy = "status"
        });

        Assert.Equal(2, all_tickets.TotalCount);
        Assert.Equal(1, all_tickets.Items.Count(t => t.Status == "Open"));
        Assert.Equal(1, all_tickets.Items.Count(t => t.Status == "In Progress"));
        Assert.Equal("In Progress", all_tickets.Items.First().Status);
        Assert.Equal("Open", all_tickets.Items.Last().Status);
    }
    
    [Trait("Category", "GET")]
    [Fact]
    public async Task GetTickets_ShouldReturn_Open_Tickets()
    {
        var context = CreateContext();
        var repo = new TicketRepository(context);
        //add tickets
        foreach (var ticket in TicketDataHelper.TicketData())
        {
            repo.AddTicket(ticket);
        }

        await context.SaveChangesAsync();

        // verify
        var all_tickets = await repo.GetTicketsAsync(new TicketParams()
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "status",
            CreatedBy = 1
        });

        Assert.Equal(2, all_tickets.TotalCount);
        Assert.Equal(1, all_tickets.Items.Count(t => t.Status == "Open"));
        Assert.Equal(1, all_tickets.Items.Count(t => t.Status == "In Progress"));
        Assert.Equal("In Progress", all_tickets.Items.First().Status);
        Assert.Equal("Open", all_tickets.Items.Last().Status);
    }

    [Trait("Category", "GET")]
    [Fact]
    public async Task GetTicketById_ShouldReturnMatchingTicket()
    {
        var repo = new TicketRepository(CreateContext());
        var ticket = new Ticket()
        {
            Title = "Unable to connect to AWS workspace",
            Description = "Service not available - error",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 102,
            CreatedBy = 1
        };
        repo.AddTicket(ticket);
        await repo.SaveChangesAsync();

        var matching_ticket = await repo.GetTicketByIdAsync(1);
        Assert.NotNull(matching_ticket);
        Assert.Equal(1, matching_ticket.Id);
        Assert.Equal("Unable to connect to AWS workspace", matching_ticket.Title);
    }

    [Trait("Category", "GET")]
    [Fact]
    public async Task GetTicketById_TicketNotFound_ShouldReturnNull()
    {
        var repo = new TicketRepository(CreateContext());
        var ticket = new Ticket()
        {
            Title = "Unable to connect to AWS workspace",
            Description = "Service not available - error",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 102,
            CreatedBy = 1
        };
        repo.AddTicket(ticket);
        await repo.SaveChangesAsync();

        var matching_ticket = await repo.GetTicketByIdAsync(100);
        Assert.Null(matching_ticket);
    }

    [Trait("Category", "GET")]
    [Fact]
    public async Task GetTickets_FilteredBySearchTerm_ShouldReturnMatchingTickets()
    {
        var repo = new TicketRepository(CreateContext());
        repo.AddTicket(new Ticket()
        {
            Title = "AWS workspace not responding",
            Description = "Service not available - error",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1
        });
        repo.AddTicket(new Ticket()
        {
            Title = "Facing Issue with HDMI port",
            Description = "No monitor detected, tried multiple HDMI cables/ports but no luck",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1
        });
        await repo.SaveChangesAsync();
        
        //get tickets - by SearchTerm
        var matchingTickets = await repo.GetTicketsAsync(new TicketParams()
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "HDMI"
        });

        Assert.NotNull(matchingTickets);
        Assert.Equal(1, matchingTickets.TotalCount);
        Assert.Equal("Open", matchingTickets.Items.First().Status);
        Assert.Equal("Facing Issue with HDMI port", matchingTickets.Items.First().Title);
        Assert.Contains("HDMI cables/ports", matchingTickets.Items.First().Description);
    }
    
    [Trait("Category", "GET")]
    [Fact]
    public async Task GetTickets_PageSizeToZero_ShouldReturnDetailsBasedOnDefaultConfigurations()
    {
        var repo = new TicketRepository(CreateContext());
        repo.AddTicket(new Ticket()
        {
            Title = "AWS workspace not responding",
            Description = "Service not available - error",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1
        });
        repo.AddTicket(new Ticket()
        {
            Title = "Facing Issue with HDMI port",
            Description = "No monitor detected, tried multiple HDMI cables/ports but no luck",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1
        });
        await repo.SaveChangesAsync();
        
        //get tickets
        var matchingTickets = await repo.GetTicketsAsync(new TicketParams()
        {
            PageNumber = 2,
            PageSize = 1
        });

        Assert.NotNull(matchingTickets);
        Assert.Equal(2, matchingTickets.TotalCount);
    }
    
    #endregion
    
    #region POST
    // void AddTicket(Ticket ticket);
    [Trait("Category", "POST")]
    [Fact]
    public async Task AddTicket_ShouldCreateNewTicket()
    {
        var context = CreateContext();
        var ticket = new Ticket()
        {
            Title = "Unable to connect to AWS workspace",
            Description = "Service not available - error",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 102,
            CreatedBy = 1
        };
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();
        testOutputHelper.WriteLine("ticket added");

        var repo = new TicketRepository(context);
        // filter by createdBy 1
        var all_tickets = await repo.GetTicketsAsync(new TicketParams()
        {
            PageNumber = 1,
            PageSize = 2,
            CreatedBy = 1
        });

        testOutputHelper.WriteLine(JsonSerializer.Serialize(all_tickets));
        Assert.Equal(1, all_tickets.TotalCount);
        Assert.Equal("Open", all_tickets.Items.First().Status);
    }
    
    [Trait("Category", "POST")]
    [Fact]
    public async Task AddTickets_ShouldCreateNewTickets()
    {
        var repo = new TicketRepository(CreateContext());
        foreach (var ticket in TicketDataHelper.TicketData())
        {
            repo.AddTicket(ticket);
        }
        await repo.SaveChangesAsync();

        var tickets = await repo.GetTicketsAsync(new TicketParams());
        Assert.NotNull(tickets);
        Assert.Equal(4, tickets.TotalCount);
        
        //Default order by createdAt ascending.
        Assert.Equal("Password reset not working for admin portal", tickets.Items.First().Title);
    }
    #endregion
    
    #region PUT
    
    [Trait("Category", "PUT")]
    [Fact]
    public async Task UpdateTicket_ShouldProperlyUpdateTicket()
    {
        var repo = new TicketRepository(CreateContext());
        
        //add ticket
        repo.AddTicket(new Ticket()
        {
            Title = "Unable to connect to AWS workspace",
            Description = "Service not available - error",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 102,
            CreatedBy = 1
        });
        await repo.SaveChangesAsync();
        
        var ticket = await repo.GetTicketByIdAsync(1);
        Assert.NotNull(ticket);
        Assert.Equal("Open", ticket.Status);
        
        // update ticket
        ticket.Status = "In Progress";
        repo.UpdateTicket(ticket);
        await repo.SaveChangesAsync();
        
        var updatedTicket = await repo.GetTicketByIdAsync(1);
        Assert.NotNull(updatedTicket);
        Assert.Equal("In Progress", updatedTicket.Status);
    }
    #endregion
    
    #region DELETE
    [Trait("Category", "DELETE")]
    [Fact]
    public async Task DeleteTicket_ShouldDeleteTicket()
    {
        var repo = new TicketRepository(CreateContext());
        
        //add ticket
        foreach (var ticket in TicketDataHelper.TicketData())
        {
            repo.AddTicket(ticket);
        }
        await repo.SaveChangesAsync();
        
        var totalTickets = await repo.GetTicketsAsync(new TicketParams());
        Assert.NotNull(totalTickets);
        Assert.Equal(4, totalTickets.TotalCount); //total 4 tickets
        
        var secondTicket = await repo.GetTicketByIdAsync(2);
        Assert.NotNull(secondTicket);
        
        // delete ticket 
        repo.DeleteTicket(secondTicket);
        await repo.SaveChangesAsync();
        
        var totalTicketsAfterDelete = await repo.GetTicketsAsync(new TicketParams());
        Assert.NotNull(totalTicketsAfterDelete);
        Assert.Equal(3, totalTicketsAfterDelete.TotalCount);
    }
    #endregion
    
    #region TicketExists

    [Trait("Category", "TicketExists")]
    [Fact]
    public async Task TicketExists_ShouldReturnTrue()
    {
        var repo = new TicketRepository(CreateContext());
        repo.AddTicket(new Ticket()
        {
            Title = "Unable to connect to AWS workspace",
            Description = "Service not available - error",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 102,
            CreatedBy = 1
        });
        await repo.SaveChangesAsync();
        var isTicketExists = await repo.TicketExists(1);
        Assert.True(isTicketExists);
    }
    
    [Trait("Category", "TicketExists")]
    [Fact]
    public async Task TicketExists_TicketNotFound_ShouldReturnFalse()
    {
        var repo = new TicketRepository(CreateContext());
        repo.AddTicket(new Ticket()
        {
            Title = "Unable to connect to AWS workspace",
            Description = "Service not available - error",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 102,
            CreatedBy = 1
        });
        await repo.SaveChangesAsync();
        var isTicketExists = await repo.TicketExists(3);
        Assert.False(isTicketExists);
    }
    #endregion
}