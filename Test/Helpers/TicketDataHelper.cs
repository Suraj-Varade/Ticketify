using Core.Entities;

namespace Test.Helpers;

public class TicketDataHelper
{
    public static List<Ticket> TicketData()
    {
        // Sample 1
        var ticket1 = new Ticket()
        {
            Title = "Password reset not working for admin portal",
            Description =
                "Users are reporting that the 'Forgot Password' link on the admin portal returns a 500 error. Tried multiple browsers - same issue.",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 105,
            CreatedBy = 1
        };

        // Sample 2
        var ticket2 = new Ticket()
        {
            Title = "Application crashes on iOS 16 devices",
            Description =
                "Mobile app crashes immediately after login on iPhone 14 Pro running iOS 16.3. Android version works fine. Crash logs attached.",
            CreatedAt = DateTime.UtcNow,
            Status = "In Progress",
            AssignTo = 108,
            CreatedBy = 1
        };

        // Sample 3
        var ticket3 = new Ticket()
        {
            Title = "Slow database queries on production server",
            Description =
                "Dashboard is taking 30+ seconds to load. Multiple users affected. Database CPU usage is at 85%. Needs urgent attention.",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 102,
            CreatedBy = 2
        };

        // Sample 4
        var ticket4 = new Ticket()
        {
            Title = "Email notifications not being sent",
            Description =
                "Users report not receiving order confirmation emails since yesterday evening. SMTP logs show 'Authentication failed' errors.",
            CreatedAt = DateTime.UtcNow,
            Status = "Open",
            AssignTo = 110,
            CreatedBy = 5
        };
        var ticket5 = new Ticket()
        {
            Title = "No able to login to workspace",
            Description =
                "Users report not receiving order confirmation emails since yesterday evening. SMTP logs show 'Authentication failed' errors.",
            CreatedAt = DateTime.UtcNow,
            Status = "Resolved",
            AssignTo = 110,
            CreatedBy = 1
        };
        List<Ticket> tickets = new List<Ticket>() { ticket1, ticket2, ticket3, ticket4 };
        return tickets;
    }
}