using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
