using Auth.Api.Models;

namespace Auth.Api.Services.Contracts
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUser);
    }
}
