using LegalDocManagement.API.Data.Models;

namespace LegalDocManagement.API.Services
{
    public interface ITokenService
    {
        string CreateToken(AppUser user, IList<string> roles);
    }
} 