using LegalDocManagement.API.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegalDocManagement.API.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created and migrations are applied
            // Note: In a production scenario, migrations are typically handled by deployment scripts.
            // For development, this ensures the DB is ready.
            // await dbContext.Database.MigrateAsync(); // We've already done this manually for InitialCreate

            // Define roles
            string[] roleNames = { "Admin", "Uploader", "LegalApprover", "ManagerApprover", "FinalApprover" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles and seed them to the database
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Get admin user details from configuration
            var adminEmail = configuration["AppSettings:AdminUserEmail"] ?? "admin@example.com";
            var adminPassword = configuration["AppSettings:AdminUserPassword"] ?? "AdminPa$$w0rd"; // Store securely!

            // Check if admin user exists
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // Or implement email confirmation
                };

                var createUserResult = await userManager.CreateAsync(newAdminUser, adminPassword);
                if (createUserResult.Succeeded)
                {
                    // Assign Admin role to the new admin user
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
                // Log errors if createUserResult failed (omitted for brevity here)
            }
        }
    }
} 