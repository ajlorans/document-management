using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalDocManagement.API.Data.Models
{
    // Represents a specific step in an approval workflow (e.g., "Legal Review", "Manager Signoff")
    // We might make this more complex later, defining sequences, etc.
    public class ApprovalStep
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string StepName { get; set; } = string.Empty;

        // The role required to complete this step
        [Required]
        public string RequiredRoleId { get; set; } = string.Empty;
        [ForeignKey("RequiredRoleId")]
        public virtual IdentityRole? RequiredRole { get; set; }

        public int Order { get; set; } // Defines the sequence of the step

        // Add other workflow-related properties if needed, e.g., WorkflowDefinitionId
    }
} 