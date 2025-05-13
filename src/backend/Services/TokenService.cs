using LegalDocManagement.API.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LegalDocManagement.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
            var secretKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.");
            _issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration.");
            _audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration.");
            
            // Ensure the key is long enough for the algorithm (HS256 requires >= 128 bits / 16 bytes)
            if(Encoding.UTF8.GetBytes(secretKey).Length < 16)
            {
                 throw new InvalidOperationException("JWT Key must be at least 16 characters long.");
            }
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        public string CreateToken(AppUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id), // Or JwtRegisteredClaimNames.Sub
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName ?? string.Empty) // Use UserName or add FirstName/LastName
                // Add other claims as needed
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature); // Use HmacSha512 for longer keys if needed

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time (e.g., 1 hour)
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
} 