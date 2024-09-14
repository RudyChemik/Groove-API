using Microsoft.AspNetCore.Identity;

namespace Groove.Models
{
    public class LoginResponse
    {
        public IdentityResult IdentityResult { get; set; }
        public string JwtToken { get; set; }
    }
}
