using Microsoft.AspNetCore.Identity;

// Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Core.Entities
{
    public class AppUser: IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}