using LegalDocManagement.API.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LegalDocManagement.API.Data
{
    // Inherit from IdentityDbContext to include Identity tables (Users, Roles, etc.)
    // Specify our AppUser and IdentityRole classes
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties for our custom models
        public DbSet<Document> Documents { get; set; }
        public DbSet<ApprovalStep> ApprovalSteps { get; set; }
        public DbSet<Approval> Approvals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Important: Call base method first

            // Configure relationships and constraints here if needed
            // Example: Define cascade delete behavior, indexes, etc.

            // Example: Configure the relationship between Document and AppUser (UploadedBy)
            builder.Entity<Document>()
                .HasOne(d => d.UploadedBy)
                .WithMany() // Assuming AppUser doesn't have a direct navigation property back to Documents
                .HasForeignKey(d => d.UploadedById)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a user if they uploaded documents

            // Example: Configure the relationship between Approval and AppUser (ApprovedBy)
             builder.Entity<Approval>()
                .HasOne(a => a.ApprovedBy)
                .WithMany() // Assuming AppUser doesn't have a direct navigation property back to Approvals
                .HasForeignKey(a => a.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a user if they approved/rejected

             // Configure relationship between Approval and Document
            builder.Entity<Approval>()
                .HasOne(a => a.Document)
                .WithMany(d => d.Approvals) // Use the navigation property in Document
                .HasForeignKey(a => a.DocumentId)
                .OnDelete(DeleteBehavior.Cascade); // Delete approvals if the document is deleted

            // Configure relationship between Approval and ApprovalStep
             builder.Entity<Approval>()
                .HasOne(a => a.ApprovalStep)
                .WithMany() // Assuming ApprovalStep doesn't have a direct navigation property back
                .HasForeignKey(a => a.ApprovalStepId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a step if it's used in approvals

             // Configure relationship between ApprovalStep and IdentityRole
            builder.Entity<ApprovalStep>()
                .HasOne(s => s.RequiredRole)
                .WithMany()
                .HasForeignKey(s => s.RequiredRoleId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a role if it's used in steps
        }
    }
} 