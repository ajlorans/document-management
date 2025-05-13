using LegalDocManagement.API.Data.Models;
using LegalDocManagement.API.DTOs;
using LegalDocManagement.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Needed to assign roles
        private readonly ILogger<AccountController> _logger;
        private readonly ITokenService _tokenService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager, 
            ILogger<AccountController> logger,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "User already exists with that email." });
            }

            var newUser = new AppUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email, // Typically use email as username
                EmailConfirmed = true // Set to false if you want email verification
            };

            var result = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("User creation failed for {Email}: {Errors}", registerDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                // Return specific errors or a generic message
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = $"User creation failed: {string.Join(", ", errors)}" });
            }

            // Assign a default role (e.g., "Uploader")
            // Ensure the role exists (SeedData should handle this, but double-check)
            string defaultRole = "Uploader";
            if (await _roleManager.RoleExistsAsync(defaultRole))
            {
                await _userManager.AddToRoleAsync(newUser, defaultRole);
            }
            else
            {
                _logger.LogWarning("Default role '{DefaultRole}' not found for user {Email}.", defaultRole, newUser.Email);
                // Handle appropriately - maybe return success but log the issue, or fail registration?
            }
            
            _logger.LogInformation("User {Email} registered successfully.", newUser.Email);
            
            // Return a success response (consider logging the user in immediately or requiring login)
            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Registration successful. Please log in." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed for non-existent email: {Email}", loginDto.Email);
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false); // Set lockoutOnFailure to true for security

            if (!result.Succeeded)
            {
                 _logger.LogWarning("Invalid login attempt for email: {Email}", loginDto.Email);
                 // Note: Add lockout logic here if needed
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." });
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var tokenString = _tokenService.CreateToken(user, roles);

            _logger.LogInformation("User {Email} logged in successfully.", user.Email);

            // Return token and user info
            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = tokenString,
                // TokenExpiration = ? // You might want to calculate and return this based on the token service logic
                UserInfo = new UserInfoDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            });
        }

    }
} 