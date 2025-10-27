using System.Net;
using API.Controllers;
using Core.Entities;
using Core.Interfaces;
using Core.RequestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test;

public class TicketControllerTests
{
    #region GET

    [Trait("TicketController", "GET")]
    [Fact]
    public async Task GetTicket_WithValidId_ReturnsOkResult()
    {
        //arrange
        var mockRepo = new Mock<ITicketRepository>();

        mockRepo.Setup(x => x.GetTicketByIdAsync(1))
            .ReturnsAsync(
                new Ticket()
                {
                    Title = "Test Ticket",
                    Status = "Open",
                    Description = "Test Description",
                    CreatedBy = 1,
                    Id = 1
                });

        var ticketController = new TicketsController(mockRepo.Object);

        //act
        var response = await ticketController.GetTicket(1);

        //assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var ticket = Assert.IsType<Ticket>(okResult.Value);
        Assert.Equal(1, ticket.Id);
        Assert.Equal("Test Ticket", ticket.Title);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Trait("TicketController", "GET")]
    [Fact]
    public async Task GetTicket_WithInvalidId_ReturnsNotFoundResult()
    {
        var mockrepo = new Mock<ITicketRepository>();
        mockrepo.Setup(x => x.GetTicketByIdAsync(1)).ReturnsAsync(new Ticket()
        {
            Title = "Test Ticket",
            Status = "Open",
            Description = "Test Description",
            CreatedBy = 1,
            Id = 1
        });
        var ticketController = new TicketsController(mockrepo.Object);
        var response = await ticketController.GetTicket(3);

        Assert.IsType<ActionResult<Ticket>>(response);
        var notFoundResponse = Assert.IsType<NotFoundResult>(response.Result);
        Assert.Equal(404, notFoundResponse.StatusCode);
    }

    #endregion

    #region POST

    [Fact]
    [Trait("TicketController", "POST")]
    public async Task AddTicket_WithValidModel_ReturnsCreatedResult()
    {
        var mockRepo = new Mock<ITicketRepository>();

        mockRepo.Setup(t => t.AddTicket(It.IsAny<Ticket>()));
        mockRepo.Setup(t => t.SaveChangesAsync()).ReturnsAsync(true);
        
        var ticketController = new TicketsController(mockRepo.Object);
        var result = await ticketController.AddTicket(new Ticket()
        {
            Title = "Test Ticket",
            Status = "Open",
            Description = "Test Description",
            CreatedBy = 1
        });
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        
        mockRepo.Verify(x => x.AddTicket(It.IsAny<Ticket>()), Times.Once);
        mockRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    [Trait("TicketController", "POST")]
    public async Task AddTicket_WithInvalidModel_ReturnsBadRequestResult()
    {
        var mockRepo = new Mock<ITicketRepository>();
        var ticketController = new TicketsController(mockRepo.Object);
        ticketController.ModelState.AddModelError("Title", "Title is required");
        ticketController.ModelState.AddModelError("Status", "Status is required");
        var result = await ticketController.AddTicket(new Ticket()
        {
            Description = "Test Description",
            CreatedBy = 1
        });
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badResult.StatusCode);
    }

    [Fact]
    [Trait("TicketController", "POST")]
    public async Task AddTicket_WhenSaveFails_ReturnsBadRequestResult()
    {
        var mockRepo = new Mock<ITicketRepository>();
        mockRepo.Setup(t => t.AddTicket(It.IsAny<Ticket>()));
        mockRepo.Setup(t => t.SaveChangesAsync()).ReturnsAsync(false);
        
        var ticketController = new TicketsController(mockRepo.Object);
        var response = await ticketController.AddTicket(new Ticket()
        {
            Title = "Test Ticket",
            Status = "Open",
            Description = "Test Description",
            CreatedBy = 1
        });

        var badResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(400, badResult.StatusCode);
    }
    
    #endregion POST
    
    #region PUT

    [Fact]
    [Trait("TicketController", "PUT")]
    public async Task UpdateTicket_WithValidModel_ReturnsNoContentResult()
    {
        //Arrange
        var mockRepo = new Mock<ITicketRepository>();
        
        //Mock that ticket exists.
        mockRepo.Setup(t => t.TicketExists(1)).ReturnsAsync(true);
        
        mockRepo.Setup(t => t.UpdateTicket(It.IsAny<Ticket>()));
        mockRepo.Setup(t => t.SaveChangesAsync()).ReturnsAsync(true);
        
        var ticketController  = new TicketsController(mockRepo.Object);
        
        var response = await ticketController.UpdateTicket(1, new Ticket()
        {
            Title = "Test Ticket",
            Status = "Completed",
            Description = "Test Description",
            CreatedBy = 1,
            Id = 1
        });
        
        var noContentResult = Assert.IsType<NoContentResult>(response);
        Assert.Equal(204, noContentResult.StatusCode);
    }
    
    [Fact]
    [Trait("TicketController", "PUT")]
    public async Task UpdateTicket_WithInValidModel_ReturnsBadResult()
    {
        //Arrange
        var mockRepo = new Mock<ITicketRepository>();
        
        //Mock that ticket exists.
        mockRepo.Setup(t => t.TicketExists(1)).ReturnsAsync(true);
        
        mockRepo.Setup(t => t.UpdateTicket(It.IsAny<Ticket>()));
        mockRepo.Setup(t => t.SaveChangesAsync()).ReturnsAsync(true);
        
        var ticketController  = new TicketsController(mockRepo.Object);
        
        var response = await ticketController.UpdateTicket(1, new Ticket());
        
        var badResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(400, badResult.StatusCode);
        Assert.Equal("cannot update this ticket", Convert.ToString(badResult.Value));
    }
    
    [Fact]
    [Trait("TicketController", "PUT")]
    public async Task UpdateTicket_WithSaveFails_ReturnsBadResult()
    {
        //Arrange
        var mockRepo = new Mock<ITicketRepository>();
        
        //Mock that ticket exists.
        mockRepo.Setup(t => t.TicketExists(1)).ReturnsAsync(true);
        mockRepo.Setup(t => t.UpdateTicket(It.IsAny<Ticket>()));
        mockRepo.Setup(t => t.SaveChangesAsync()).ReturnsAsync(false);
        
        var ticketController  = new TicketsController(mockRepo.Object);
        
        var response = await ticketController.UpdateTicket(1, new Ticket()
        { 
            Title = "Test Ticket",
            Status = "Completed",
            Description = "Test Description",
            CreatedBy = 1,
            Id = 1
        });
        
        var badResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(400, badResult.StatusCode);
        Assert.Equal("problem updating ticket", Convert.ToString(badResult.Value));
    }

    [Fact]
    [Trait("TicketController", "PUT")]
    public async Task UpdateTicket_WithIdMisMatch_ReturnsBadResult()
    {
        var mockRepo = new Mock<ITicketRepository>();
        //mockRepo.Setup(t => t.AddTicket(It.IsAny<Ticket>()));
        //mockRepo.Setup(t => t.TicketExists(1)).ReturnsAsync(true);
        
        var ticketController = new TicketsController(mockRepo.Object);
        var response = await ticketController.UpdateTicket(1, new Ticket()
        {
            Title = "Sample Title",
            Status = "Completed",
            Description = "Sample Description",
            CreatedBy = 1,
            Id = 3
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal("cannot update this ticket", badRequest.Value);
    }
    
    #endregion PUT
    
    #region DELETE

    [Fact]
    [Trait("TicketController", "DELETE")]
    public async Task DeleteTicket_WithInvalidId_ReturnsNotfoundResult()
    {
        var mockRepo = new Mock<ITicketRepository>();
        mockRepo.Setup(t => t.GetTicketByIdAsync(1)).ReturnsAsync(new Ticket()
        {
            Title = "ticket title",
            Description = "Description",
            CreatedBy = 1,
            Status = "Resolved"
        });

        var ticketController = new TicketsController(mockRepo.Object);
        var response = await ticketController.DeleteTicket(3);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("ticket not found", notFoundResult.Value);
    }
    
    [Fact]
    [Trait("TicketController", "DELETE")]
    public async Task DeleteTicket_WithValidId_ReturnsNoContentResult()
    {
        var mockRepo = new Mock<ITicketRepository>();
        mockRepo.Setup(t => t.GetTicketByIdAsync(1)).ReturnsAsync(new Ticket()
        {
            Title = "ticket title",
            Description = "Description",
            CreatedBy = 1,
            Status = "Resolved",
            Id = 1
        });
        mockRepo.Setup(t => t.SaveChangesAsync()).ReturnsAsync(true);

        var ticketController = new TicketsController(mockRepo.Object);
        var response = await ticketController.DeleteTicket(1);

        var noContentResult = Assert.IsType<NoContentResult>(response);
        Assert.Equal(204, noContentResult.StatusCode);
    }
    
    [Fact]
    [Trait("TicketController", "DELETE")]
    public async Task DeleteTicket_WithSaveFails_ReturnsBadResult()
    {
        var mockRepo = new Mock<ITicketRepository>();
        mockRepo.Setup(t => t.GetTicketByIdAsync(1)).ReturnsAsync(new Ticket()
        {
            Title = "ticket title",
            Description = "Description",
            CreatedBy = 1,
            Status = "Resolved",
            Id = 1
        });
        mockRepo.Setup(t => t.SaveChangesAsync()).ReturnsAsync(false);

        var ticketController = new TicketsController(mockRepo.Object);
        var response = await ticketController.DeleteTicket(1);

        var badResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(400, badResult.StatusCode);
        Assert.Equal("problem deleting ticket", badResult.Value);
    }
    #endregion
}