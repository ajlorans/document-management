namespace LegalDocManagement.API.Data.Models
{
    public enum ApprovalStatus
    {
        Pending,      // Initial state after upload
        InProgress,   // Actively going through approval steps
        Approved,     // All required approvals received
        Rejected      // An approver rejected the document
    }
} 