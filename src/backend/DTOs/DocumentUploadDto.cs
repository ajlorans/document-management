using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LegalDocManagement.API.DTOs
{
    // DTO for uploading a new document
    public class DocumentUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!; // The actual file being uploaded

        // Add other metadata if needed during upload (e.g., description, initial tags)
        public string? Description { get; set; }
    }
} 