using Groove.Data;
using System.Security.Claims;

namespace ElGatoAPI.Data
{
    public class JwtService : IJwtService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserIdClaim()
        {
            var identity = _httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                throw new InvalidOperationException("User identity is not available.");
            }

            var userIdClaim = identity.FindFirst("UserId");
            if (userIdClaim == null)
            {
                throw new InvalidOperationException("User ID claim not found.");
            }

            return userIdClaim.Value;
        }
    }
}
