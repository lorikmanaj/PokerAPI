using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelService;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DataService;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace FiltersService
{
    public class AdminAuthenticationHandler : AuthenticationHandler<AdminAuthenticationOptions>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _provider;
        private readonly IdentityDefaultOptions _identityDefaultOptions;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly AppSettings _appSettings;

        private const string AccessToken = "access_token";
        private const string User_Id = "user_id";
        private const string Username = "username";
        private readonly string[] UserRoles = new[] { "Administrator" };

        public AdminAuthenticationHandler(IOptionsMonitor<AdminAuthenticationOptions> options,
                                            ILoggerFactory logger,
                                            UrlEncoder encoder,
                                            ISystemClock clock,
                                            UserManager<ApplicationUser> userManager,
                                            IOptions<AppSettings> appSettings,
                                            IOptions<DataProtectionKeys> dataProtectionKeys,
                                            IServiceProvider provider,
                                            IOptions<IdentityDefaultOptions> identityDefaultOptions)
                                        : base(options, logger, encoder, clock)
        {
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _identityDefaultOptions = identityDefaultOptions.Value;
            _provider = provider;
            _dataProtectionKeys = dataProtectionKeys.Value;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Cookies.ContainsKey(AccessToken) || !Request.Cookies.ContainsKey(User_Id))
            {
                Log.Error("No Access Token or User Id found.");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!AuthenticationHeaderValue.TryParse($"{"Bearer " + Request.Cookies[AccessToken]}",
                out AuthenticationHeaderValue headerValue))
            {
                Log.Error("Could not Parse Token from Authentication Header.");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!AuthenticationHeaderValue.TryParse($"{"Bearer " + Request.Cookies[User_Id]}",
                out AuthenticationHeaderValue headerValueUid))
            {
                Log.Error("Could not Parse User Id from Authentication Header.");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            try
            {
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var handler = new JwtSecurityTokenHandler();

                TokenValidationParameters validationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = _appSettings.Site,
                        ValidAudience = _appSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                var protectorProvider = _provider.GetService<IDataProtectionProvider>();

                var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                var decryptedUid = protector.Unprotect(headerValueUid.Parameter);
                var decryptedToken = protector.Unprotect(headerValue.Parameter);

                TokenModel tokenModel = new TokenModel();

                using (var scope = _provider.CreateScope())
                {
                    var dbContextService = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    var userToken = dbContextService.Tokens.Include(_ => _.User)
                        .FirstOrDefault(_ => _.UserId == decryptedUid
                        && _.User.UserName == Request.Cookies[Username]
                        && _.User.Id == decryptedUid
                        && _.User.UserRole == "Administrator");
                    tokenModel = userToken;
                }

                if (tokenModel == null)
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to View this Page"));

                IDataProtector layerTwoProtector = protectorProvider.CreateProtector(tokenModel?.EncryptionKeyJwt);
                string decryptedTokenLayerTwo = layerTwoProtector.Unprotect(decryptedToken);

                var validateToken = handler.ValidateToken(decryptedTokenLayerTwo, validationParameters, out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized"));

                var username = validateToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                if (Request.Cookies[Username] != username)
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized"));

                var user = await _userManager.FindByNameAsync(username);

                if (user == null)
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to View this Page"));

                if (!UserRoles.Contains(user.UserRole))
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to View this Page"));

                var identity = new ClaimsIdentity(validateToken.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return await Task.FromResult(AuthenticateResult.Fail("You are not authorized"));
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            //return base.HandleChallengeAsync(properties);
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("user_id");
            Response.Headers["WWW-Authenticate"] = $"Not Authorized";
            Response.Redirect(_identityDefaultOptions.AccessDeniedPath);

            return Task.CompletedTask;
        }
    }
}
