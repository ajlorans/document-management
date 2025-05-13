using Microsoft.AspNetCore.Identity;

namespace LegalDocManagement.API.Data.Models
{
    // Inherit from IdentityUser to leverage ASP.NET Core Identity features
    // We can add custom properties here later if needed (e.g., FullName, Department)
    public class AppUser : IdentityUser
    {
        // Example custom property:
        // public string? FullName { get; set; } 
    }
} 