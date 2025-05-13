using LegalDocManagement.API.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace LegalDocManagement.API.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            // var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>(); // dbContext not used in current logic, commented out to avoid unused variable warning
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>(); // Get ILoggerFactory
            var logger = loggerFactory.CreateLogger("SeedData"); // Create logger with a category name

            logger.LogInformation("SeedData.Initialize started.");

            // Define roles
            string[] roleNames = { "Admin", "Uploader", "LegalApprover", "ManagerApprover", "FinalApprover" };
            logger.LogInformation("Ensuring roles exist: {Roles}", string.Join(", ", roleNames));

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    logger.LogInformation("Role '{RoleName}' not found. Creating it.", roleName);
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                    }
                    else
                    {
                        logger.LogError("Error creating role '{RoleName}': {Errors}", roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Role '{RoleName}' already exists.", roleName);
                }
            }

            // Get admin user details from configuration
            var adminEmail = configuration["AppSettings:AdminUserEmail"] ?? "admin@example.com";
            var adminPassword = configuration["AppSettings:AdminUserPassword"] ?? "AdminPa$$w0rd";
            logger.LogInformation("Admin user email from configuration: {AdminEmail}", adminEmail);

            // Check if admin user exists
            logger.LogInformation("Checking if admin user '{AdminEmail}' exists...", adminEmail);
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                logger.LogInformation("Admin user '{AdminEmail}' not found. Attempting to create.", adminEmail);
                var newAdminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // Or implement email confirmation
                };

                var createUserResult = await userManager.CreateAsync(newAdminUser, adminPassword);
                if (createUserResult.Succeeded)
                {
                    logger.LogInformation("Admin user '{AdminEmail}' created successfully. Assigning Admin role.", adminEmail);
                    var addToRoleResult = await userManager.AddToRoleAsync(newAdminUser, "Admin");
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("Successfully assigned Admin role to '{AdminEmail}'.", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Error assigning Admin role to '{AdminEmail}': {Errors}", adminEmail, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogError("Error creating admin user '{AdminEmail}': {Errors}", adminEmail, string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Admin user '{AdminEmail}' already exists. Ensuring Admin role assignment and updating password.", adminEmail);
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    logger.LogInformation("Admin user '{AdminEmail}' exists but does not have Admin role. Assigning Admin role.", adminEmail);
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("Successfully assigned Admin role to existing user '{AdminEmail}'.", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Error assigning Admin role to existing user '{AdminEmail}': {Errors}", adminEmail, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Existing admin user '{AdminEmail}' already has Admin role.", adminEmail);
                }

                logger.LogInformation("Attempting to update password for admin user '{AdminEmail}'.", adminEmail);
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetPasswordResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);

                if (resetPasswordResult.Succeeded)
                {
                    logger.LogInformation("Successfully updated password for admin user '{AdminEmail}'.", adminEmail);
                }
                else
                {
                    logger.LogError("Error updating password for admin user '{AdminEmail}': {Errors}", adminEmail, string.Join(", ", resetPasswordResult.Errors.Select(e => e.Description)));
                }
            }
            logger.LogInformation("SeedData.Initialize finished.");
        }
    }
} 