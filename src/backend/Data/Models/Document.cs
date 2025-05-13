using System.ComponentModel.DataAnnotations;

namespace LegalDocManagement.API.Data.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [StringLength(1000)] // Allow a longer description
        public string? Description { get; set; } // Made nullable as it might not always be provided

        // Store file path/URL instead of the file itself in the DB
        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string UploadedById { get; set; } = string.Empty;
        public virtual AppUser? UploadedBy { get; set; }

        public ApprovalStatus CurrentStatus { get; set; } = ApprovalStatus.Pending;

        // Navigation property for related approvals
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }
} 