namespace LegalDocManagement.API.Data.Models
{
    public enum ApprovalStatus
    {
        Pending,      // Initial state after upload
        AwaitingLegal,  // Waiting for Legal Approver
        AwaitingManager, // Waiting for Manager Approver
        AwaitingFinal,  // Waiting for Final Approver
        InProgress,   // Actively going through approval steps (may be redundant if specific states are used)
        Approved,     // All required approvals received
        Rejected      // An approver rejected the document
    }
} 