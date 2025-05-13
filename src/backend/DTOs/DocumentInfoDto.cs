using LegalDocManagement.API.Data.Models; // For ApprovalStatus enum

namespace LegalDocManagement.API.DTOs
{
    // DTO for returning document information
    public class DocumentInfoDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedByEmail { get; set; } = string.Empty; // Show email instead of ID
        public ApprovalStatus CurrentStatus { get; set; }
        // Add other relevant info like current approval step name if needed later
    }
} 