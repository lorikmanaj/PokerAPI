using ModelService;
using System;
using System.Security.Claims;

namespace AuthService
{
    public class TokenResponseModel
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public string RefreshToken { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public int PlayerId { get; set; }
        public bool TwoFactorLoginOn { get; set; }
        public ClaimsPrincipal Principal { get; set; }
        public ResponseStatusInfoModel ResponseInfo { get; set; }
    }
}