using LegalDocManagement.API.Data;
using LegalDocManagement.API.Data.Models;
using LegalDocManagement.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LegalDocManagement.API.Controllers
{
    // DTO for providing optional rejection comments
    public class RejectDto
    {
        public string? Comments { get; set; }
    }

    [Authorize] // Require authentication for all actions in this controller
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(ApplicationDbContext context, UserManager<AppUser> userManager, ILogger<DocumentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // POST: api/documents/upload
        [HttpPost("upload")]
        [Authorize(Roles = "Uploader,Admin")] // Only allow Uploaders or Admins to upload
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadDto uploadDto)
        {
            if (uploadDto.File == null || uploadDto.File.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // This case should ideally not happen if token is valid
                return Unauthorized(new { message = "User not found." });
            }

            var placeholderFilePath = $"uploads/{Guid.NewGuid()}_{uploadDto.File.FileName}";
            _logger.LogInformation("Simulating file save for {FileName} to path {FilePath}", uploadDto.File.FileName, placeholderFilePath);

            // Fetch all defined approval steps, ordered by their sequence
            var approvalSteps = await _context.ApprovalSteps
                                            .OrderBy(s => s.Order)
                                            .Include(s => s.RequiredRole) // Include Role for checking Role Name
                                            .ToListAsync();

            if (!approvalSteps.Any())
            {
                _logger.LogError("No approval steps defined in the system. Cannot initialize document workflow.");
                // Consider returning an error or having a default behavior
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Workflow configuration error: No approval steps defined." });
            }

            // Determine initial status based on the first step
            var firstStep = approvalSteps.First();
            ApprovalStatus initialStatus = ApprovalStatus.Pending; // Default if no steps or mapping fails

            // A more robust way to map first step role to initial status
            // This assumes SeedData creates roles like "LegalApprover", "ManagerApprover", "FinalApprover"
            switch (firstStep.RequiredRole?.Name)
            {
                case "LegalApprover":
                    initialStatus = ApprovalStatus.AwaitingLegal;
                    break;
                case "ManagerApprover":
                    initialStatus = ApprovalStatus.AwaitingManager;
                    break;
                case "FinalApprover":
                    initialStatus = ApprovalStatus.AwaitingFinal;
                    break;
                // Add other cases if more roles start workflows
                default:
                    _logger.LogWarning("First approval step role '{RoleName}' does not match expected roles for initial status. Defaulting to Pending.", firstStep.RequiredRole?.Name);
                    // Fallback to Pending if no specific mapping. Consider if this should be an error.
                    initialStatus = ApprovalStatus.Pending;
                    break;
            }


            var document = new Document
            {
                FileName = uploadDto.File.FileName,
                FilePath = placeholderFilePath,
                ContentType = uploadDto.File.ContentType,
                FileSize = uploadDto.File.Length,
                UploadedAt = DateTime.UtcNow,
                UploadedById = userId,
                Description = uploadDto.Description, // Added description
                CurrentStatus = initialStatus 
            };

            // Create initial Approval records for each step
            foreach (var step in approvalSteps)
            {
                document.Approvals.Add(new Approval
                {
                    ApprovalStepId = step.Id,
                    // ApprovedById, ActionedAt, IsApproved, Comments will be null/false initially
                });
            }

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} uploaded by User {UserId}, initial status {InitialStatus}", document.Id, userId, document.CurrentStatus);

            var resultDto = new DocumentInfoDto
            {
                Id = document.Id,
                FileName = document.FileName,
                Description = document.Description,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                UploadedAt = document.UploadedAt,
                UploadedByEmail = user.Email ?? "N/A",
                CurrentStatus = document.CurrentStatus
            };

            return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, resultDto);
        }

        // GET: api/documents
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DocumentInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<DocumentInfoDto>>> GetDocuments()
        {
             _logger.LogInformation("Fetching all documents");
            var documents = await _context.Documents
                                    .Include(d => d.UploadedBy) // Include user info to get email
                                    .OrderByDescending(d => d.UploadedAt)
                                    .Select(d => new DocumentInfoDto
                                    {
                                        Id = d.Id,
                                        FileName = d.FileName,
                                        ContentType = d.ContentType,
                                        FileSize = d.FileSize,
                                        UploadedAt = d.UploadedAt,
                                        UploadedByEmail = d.UploadedBy != null ? d.UploadedBy.Email ?? "N/A" : "N/A",
                                        CurrentStatus = d.CurrentStatus
                                    })
                                    .ToListAsync();

            return Ok(documents);
        }

        // Placeholder for GET: api/documents/{id} - needed for CreatedAtAction
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DocumentInfoDto>> GetDocumentById(int id)
        {
             _logger.LogInformation("Fetching document with ID: {DocumentId}", id);
             var document = await _context.Documents
                                    .Include(d => d.UploadedBy)
                                    .Where(d => d.Id == id)
                                    .Select(d => new DocumentInfoDto
                                    {
                                        Id = d.Id,
                                        FileName = d.FileName,
                                        ContentType = d.ContentType,
                                        FileSize = d.FileSize,
                                        UploadedAt = d.UploadedAt,
                                        UploadedByEmail = d.UploadedBy != null ? d.UploadedBy.Email ?? "N/A" : "N/A",
                                        CurrentStatus = d.CurrentStatus
                                    })
                                    .FirstOrDefaultAsync();

            if (document == null)
            {
                _logger.LogWarning("Document with ID: {DocumentId} not found.", id);
                return NotFound();
            }

            return Ok(document);
        }
        
        // POST: api/documents/{id}/approve
        [HttpPost("{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            return await HandleApprovalAction(id, true, null);
        }

        // POST: api/documents/{id}/reject
        [HttpPost("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectDocument(int id, [FromBody] RejectDto? rejectDto)
        {
            return await HandleApprovalAction(id, false, rejectDto?.Comments);
        }

        // Shared logic for Approve/Reject actions
        private async Task<IActionResult> HandleApprovalAction(int documentId, bool isApproved, string? comments)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found." });
            }

            // Find the document, including its pending approvals and related step/role info
            var document = await _context.Documents
                .Include(d => d.Approvals)
                    .ThenInclude(a => a.ApprovalStep)
                        .ThenInclude(s => s.RequiredRole)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                return NotFound(new { message = "Document not found." });
            }

            // Check if document is already in a final state
            if (document.CurrentStatus == ApprovalStatus.Approved || document.CurrentStatus == ApprovalStatus.Rejected)
            {
                return BadRequest(new { message = $"Document is already in a final state ({document.CurrentStatus})." });
            }

            // Find the current step definition based on the document's current status
            var currentStep = await _context.ApprovalSteps
                                        .Include(s => s.RequiredRole)
                                        .FirstOrDefaultAsync(s => 
                                            // This mapping logic needs to be robust
                                            (s.RequiredRole.Name == "LegalApprover" && document.CurrentStatus == ApprovalStatus.AwaitingLegal) ||
                                            (s.RequiredRole.Name == "ManagerApprover" && document.CurrentStatus == ApprovalStatus.AwaitingManager) ||
                                            (s.RequiredRole.Name == "FinalApprover" && document.CurrentStatus == ApprovalStatus.AwaitingFinal)
                                            // Add more mappings if needed
                                            // Or, ideally, store the current ApprovalStepId on the Document model
                                        );
            
            if (currentStep == null)
            {
                 _logger.LogError("Could not determine the current approval step for document {DocumentId} with status {Status}", document.Id, document.CurrentStatus);
                 return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Workflow error: Cannot determine current approval step." });
            }
            
            // Find the specific Approval record for this step and document that is still pending (no ApprovedById)
            var approvalRecord = document.Approvals
                                       .FirstOrDefault(a => a.ApprovalStepId == currentStep.Id && string.IsNullOrEmpty(a.ApprovedById));

            if (approvalRecord == null)
            {
                // This shouldn't happen if approvals were created correctly on upload
                _logger.LogError("Could not find pending approval record for document {DocumentId} and step {StepId}", document.Id, currentStep.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Workflow error: Pending approval record not found." });
            }

            // Authorization Check: Does the current user have the required role for this step?
            var requiredRoleName = currentStep.RequiredRole?.Name;
            if (string.IsNullOrEmpty(requiredRoleName) || !User.IsInRole(requiredRoleName))
            {
                 _logger.LogWarning("User {UserId} attempted to action step {StepId} for doc {DocumentId} without required role {RoleName}", userId, currentStep.Id, document.Id, requiredRoleName ?? "[Unknown]");
                 return Forbid(); // 403 Forbidden
            }

            // Update the Approval record
            approvalRecord.IsApproved = isApproved;
            approvalRecord.Comments = comments; // Can be null for approval
            approvalRecord.ApprovedById = userId;
            approvalRecord.ActionedAt = DateTime.UtcNow;

            // Update Document Status
            if (!isApproved) // Rejection
            {
                document.CurrentStatus = ApprovalStatus.Rejected;
                _logger.LogInformation("Document {DocumentId} rejected by User {UserId} at step {StepName}. Comments: {Comments}", document.Id, userId, currentStep.StepName, comments);
            }
            else // Approval
            {
                // Find the next step in the sequence
                var nextStep = await _context.ApprovalSteps
                                            .Include(s => s.RequiredRole)
                                            .Where(s => s.Order > currentStep.Order)
                                            .OrderBy(s => s.Order)
                                            .FirstOrDefaultAsync();

                if (nextStep == null) // This was the last step
                {
                    document.CurrentStatus = ApprovalStatus.Approved;
                    _logger.LogInformation("Document {DocumentId} approved by User {UserId} at final step {StepName}.", document.Id, userId, currentStep.StepName);
                }
                else // Move to the next step
                {
                    // Map next step role to next status (similar logic as in upload)
                     switch (nextStep.RequiredRole?.Name)
                    {
                        case "LegalApprover": document.CurrentStatus = ApprovalStatus.AwaitingLegal; break;
                        case "ManagerApprover": document.CurrentStatus = ApprovalStatus.AwaitingManager; break;
                        case "FinalApprover": document.CurrentStatus = ApprovalStatus.AwaitingFinal; break;
                        default:
                            _logger.LogError("Cannot determine next status for document {DocumentId}. Next step role is {RoleName}", document.Id, nextStep.RequiredRole?.Name ?? "[Unknown]");
                            // Keep current status? Revert? Set to error state?
                            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Workflow error: Cannot determine next status." });
                    }
                     _logger.LogInformation("Document {DocumentId} approved by User {UserId} at step {StepName}. Moving to status {NextStatus}", document.Id, userId, currentStep.StepName, document.CurrentStatus);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                 _logger.LogError(ex, "Error saving approval action for document {DocumentId}", document.Id);
                 return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Database error saving approval action." });
            }
            
            // Return the updated document info or just OK
            // Consider returning the updated Approval record or the new Document status
             var resultDto = new DocumentInfoDto // Reuse DTO
            {
                Id = document.Id,
                FileName = document.FileName,
                Description = document.Description,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                UploadedAt = document.UploadedAt,
                UploadedByEmail = await GetUserEmail(document.UploadedById), // Helper needed
                CurrentStatus = document.CurrentStatus
            };

            return Ok(resultDto);
        }

        // Helper method to get user email (consider moving to a service)
        private async Task<string> GetUserEmail(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return "N/A";
            var user = await _userManager.FindByIdAsync(userId);
            return user?.Email ?? "N/A";
        }

    }
} 