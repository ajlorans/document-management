using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalDocManagement.API.Data.Models
{
    // Tracks the approval of a specific document by a specific user for a specific step
    public class Approval
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public virtual Document? Document { get; set; }

        public int ApprovalStepId { get; set; }
        [ForeignKey("ApprovalStepId")]
        public virtual ApprovalStep? ApprovalStep { get; set; }

        public string? ApprovedById { get; set; } // Null if not yet actioned or if actioned by system
        [ForeignKey("ApprovedById")]
        public virtual AppUser? ApprovedBy { get; set; }

        public DateTime? ActionedAt { get; set; }

        public bool IsApproved { get; set; } // True for approved, False for rejected

        [StringLength(1000)]
        public string? Comments { get; set; }
    }
} 