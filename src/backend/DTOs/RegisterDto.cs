using System.ComponentModel.DataAnnotations;

namespace LegalDocManagement.API.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Optional: Add other fields like FirstName, LastName if needed during registration
        // public string? FirstName { get; set; }
        // public string? LastName { get; set; }
    }
} 