namespace LegalDocManagement.API.DTOs
{
    // Response sent back after successful login/registration
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; } // JWT Token
        public DateTime? TokenExpiration { get; set; }
        public UserInfoDto? UserInfo { get; set; } // Basic user info
    }

    public class UserInfoDto
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
        // Add other user details you want to send back to the client
    }
} 