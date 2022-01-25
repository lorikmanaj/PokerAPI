using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ActivityService;
using CookieService;
using DataService;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelService;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ModelService.UserUpdateViewModel;

namespace AuthService
{
    public class AuthSvc : IAuthSvc
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _db;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly ICookieSvc _cookieSvc;
        private readonly IServiceProvider _provider;
        private readonly IActivitySvc _activitySvc;
        private IDataProtector _protector;
        private readonly string[] UserRoles = new[] { "Administrator", "Player" };
        private TokenValidationParameters validationParameters;
        private JwtSecurityTokenHandler handler;
        private string unProtectedToken;
        private ClaimsPrincipal validateToken;

        public AuthSvc(UserManager<ApplicationUser> userManager,
            IOptions<AppSettings> appSettings,
            ApplicationDbContext db,
            IOptions<DataProtectionKeys> dataProtectionKeys,
            ICookieSvc cookieSvc, IServiceProvider provider, IActivitySvc activitySvc)
        {
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _db = db;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _cookieSvc = cookieSvc;
            _provider = provider;
            _activitySvc = activitySvc;
        }

        // These method will be called by Client or application Users => Angular/REST API app
        public async Task<TokenResponseModel> Auth(TokenRequestModel model)
        {
            if (model == null)
                return CreateErrorResponseToken("Model State is Invalid", HttpStatusCode.InternalServerError);

            return model.GrantType switch
            {
                "password" => await GenerateNewToken(model),
                "refresh_token" => await RefreshToken(model),
                _ => CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized),
            };
        }

        // Will be used for authenticating the Admin
        public async Task<TokenResponseModel> Auth(LoginViewModel model)
        {
            ActivityModel activityModel = new ActivityModel
            {
                Date = DateTime.UtcNow,
                IpAddress = _cookieSvc.GetUserIP(),
                Location = _cookieSvc.GetUserCountry(),
                OperatingSystem = _cookieSvc.GetUserOS()
            };

            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user == null) return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);

                var roles = await _userManager.GetRolesAsync(user);

                if (roles.FirstOrDefault() != "Administrator")
                {
                    activityModel.UserId = user.Id;
                    //activityModel.Player
                    activityModel.Type = "Un-Authorized Login attempt";
                    activityModel.Icon = "fas fa-user-secret";
                    activityModel.Color = "danger";
                    await _activitySvc.AddUserActivity(activityModel);
                    Log.Error("Error : Role Not Admin");
                    return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
                }

                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    activityModel.UserId = user.Id;
                    activityModel.Type = "Login attempt failed";
                    activityModel.Icon = "far fa-times-circle";
                    activityModel.Color = "danger";
                    await _activitySvc.AddUserActivity(activityModel);
                    Log.Error("Error : Invalid Password for Admin");
                    return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    activityModel.Type = "Login attempt Success - Email Not Verified";
                    activityModel.UserId = user.Id;
                    activityModel.Icon = "far fa-envelope";
                    activityModel.Color = "warning";
                    await _activitySvc.AddUserActivity(activityModel);
                    Log.Error("Error : Email Not Confirmed for {user}", user.UserName);
                    return CreateErrorResponseToken("Email Not Confirmed", HttpStatusCode.Unauthorized);
                }

                activityModel.UserId = user.Id;
                activityModel.Type = "Login attempt successful";
                activityModel.Icon = "fas fa-thumbs-up";
                activityModel.Color = "success";
                await _activitySvc.AddUserActivity(activityModel);
                var authToken = await GenerateNewToken(user, model);
                return authToken;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
        }


        public async Task<bool> LogoutUserAsync()
        {
            var cookiesToDelete = new[]
            {
                "memberId",
                "rememberDevice",
                "access_token",
                "loginStatus",
                "refreshToken",
                "userRole",
                "username",
                "user_id"
            };

            try
            {
                var username = _cookieSvc.Get("username");

                if (username != null)
                {
                    var user = await _userManager.FindByNameAsync(username);
                    var memberToken = await _db.Tokens.Where(x => x.UserId == user.Id).ToListAsync();

                    if (memberToken.Count > 0)
                    {
                        _db.Tokens.RemoveRange(memberToken);
                        await _db.SaveChangesAsync();
                    }

                    _cookieSvc.DeleteAllCookies(cookiesToDelete);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            _cookieSvc.DeleteAllCookies(cookiesToDelete);
            return false;
        }

        public async Task<TokenResponseModel> GenerateNewToken()
        {
            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user == null) return CreateErrorResponseToken("Something went wrong. Please try again later", HttpStatusCode.Unauthorized);

                var accessToken = await CreateAccessToken(user);

                var refreshTokenExpireTime = accessToken.RefreshTokenExpiration.Subtract(DateTime.UtcNow).TotalMinutes;
                // set cookie for jwt and refresh token
                // Expiry time for cookie - When Refresh token expires all other cookies should expire
                // therefor set all the cookie expiry time to refresh token expiry time
                _cookieSvc.SetCookie("access_token", accessToken.Token.ToString(), Convert.ToInt32(refreshTokenExpireTime));
                _cookieSvc.SetCookie("refreshToken", accessToken.RefreshToken, Convert.ToInt32(refreshTokenExpireTime));
                _cookieSvc.SetCookie("loginStatus", "1", Convert.ToInt32(refreshTokenExpireTime), false, false);
                _cookieSvc.SetCookie("username", user.UserName, Convert.ToInt32(refreshTokenExpireTime), false, false);
                _cookieSvc.SetCookie("userRole", user.UserRole, Convert.ToInt32(refreshTokenExpireTime), false, false);
                _cookieSvc.SetCookie("user_id", accessToken.UserId, Convert.ToInt32(refreshTokenExpireTime));
                
                return accessToken;

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred at GenerateNewToken(TwoFactorResponseModel model) {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
        }

        public void DeleteAllCookies(IEnumerable<string> cookiesToDelete)
        {
            _cookieSvc.DeleteAllCookies(cookiesToDelete);
        }

        public void DeleteCookie(string name)
        {
            _cookieSvc.DeleteCookie(name);
        }

        // Private Methods

        private async Task<TokenResponseModel> GenerateNewToken(TokenRequestModel model)
        {
            try
            {
                // check if there's an user with the given username
                var user = await _userManager.FindByNameAsync(model.UserName);

                // Validate credentials
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password) && user.UserRole != "Administrator")
                {
                    // If the user account is locked
                    if (user.LockoutEnabled)
                        return CreateErrorResponseToken("Account Locked", HttpStatusCode.Unauthorized);

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                        return CreateErrorResponseToken("Email Not Confirmed", HttpStatusCode.Unauthorized);
                    
                    // Create & Return the access token which contains JWT and Refresh Token
                    user.RememberMe = model.RememberMe;
                    var accessToken = await CreateAccessToken(user);

                    var expireTime = accessToken.Expiration.Subtract(DateTime.UtcNow).TotalMinutes;
                    var refreshTokenExpireTime = accessToken.RefreshTokenExpiration.Subtract(DateTime.UtcNow).TotalMinutes;

                    // set cookie for jwt and refresh token
                    _cookieSvc.SetCookie("access_token", accessToken.Token.ToString(), Convert.ToInt32(refreshTokenExpireTime));
                    _cookieSvc.SetCookie("refreshToken", accessToken.RefreshToken, Convert.ToInt32(refreshTokenExpireTime));
                    _cookieSvc.SetCookie("loginStatus", "1", Convert.ToInt32(refreshTokenExpireTime), false, false);
                    _cookieSvc.SetCookie("username", user.UserName, Convert.ToInt32(refreshTokenExpireTime), false, false);
                    _cookieSvc.SetCookie("userRole", user.UserRole, Convert.ToInt32(refreshTokenExpireTime), false, false);
                    _cookieSvc.SetCookie("user_id", accessToken.UserId, Convert.ToInt32(refreshTokenExpireTime));
                    return accessToken;

                }
                return CreateErrorResponseToken("Invalid Username/Password", HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return CreateErrorResponseToken("There was an Error While Processing this request", HttpStatusCode.InternalServerError);
            }
        }

        private async Task<TokenResponseModel> CreateAccessToken(ApplicationUser user)
        {

            var tokenExpiryTime = Convert.ToDouble(_appSettings.ExpireTime);
            var rtTokenExpiryTime = Convert.ToDouble(_appSettings.RtExpireTime);

            // To validate if remember me is checked
            if (user.RememberMe)
            {
                tokenExpiryTime = 1440; // 1 day
                rtTokenExpiryTime = 1560; // + 2hours
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            var roles = await _userManager.GetRolesAsync(user);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                        new Claim("PlayerId", user.PlayerId.ToString())
                     }),

                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _appSettings.Site,
                Audience = _appSettings.Audience,
                Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
            };

            // Generate token
            /* Create the unique encryption key for token - 2nd layer protection */
            var encryptionKeyRt = Guid.NewGuid().ToString();
            var encryptionKeyJwt = Guid.NewGuid().ToString();
            /* Get the Data protection service instance */
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            /* Create a protector instance */
            var protectorJwt = protectorProvider.CreateProtector(encryptionKeyJwt);
            /* Generate Token and Protect the user token */
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            /* Create and update the token table */
            TokenModel newRtoken = new TokenModel();

            /* Create refresh token instance */
            newRtoken = CreateRefreshToken(_appSettings.ClientId, user.Id, Convert.ToInt32(rtTokenExpiryTime));

            /* assign the tne JWT encryption key */
            newRtoken.EncryptionKeyJwt = encryptionKeyJwt;

            newRtoken.EncryptionKeyRt = encryptionKeyRt;

            /* Add Refresh Token with Encryption Key for JWT to DB */
            try
            {
                // First we need to check if the user has already logged in and has tokens in DB
                var rt = await _db.Tokens
                    .Where(t => t.UserId == user.Id).ToListAsync();

                if (rt != null)
                {
                    // invalidate the old refresh token (by deleting it)
                    foreach (var oldRt in rt)
                        _db.Tokens.Remove(oldRt);

                    // add the new refresh token
                    await _db.Tokens.AddAsync(newRtoken);
                }
                else
                    await _db.Tokens.AddAsync(newRtoken);

                // persist changes in the DB
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            // Return Response containing encrypted token
            var protectorRt = protectorProvider.CreateProtector(encryptionKeyRt);

            var encAuthToken = new TokenResponseModel
            {
                Token = encryptedToken,
                Expiration = token.ValidTo,
                RefreshToken = protectorRt.Protect(newRtoken.Value),
                RefreshTokenExpiration = newRtoken.ExpiryTime,
                Role = roles.FirstOrDefault(),
                Username = user.UserName,
                UserId = protectorRt.Protect(user.Id),
                PlayerId = user.PlayerId,
                TwoFactorLoginOn = user.TwoFactorEnabled,
                ResponseInfo = CreateResponse("Login Success", HttpStatusCode.OK)
            };

            return encAuthToken;
        }

        private async Task<TokenResponseModel> RefreshToken(TokenRequestModel model)
        {
            try
            {
                if (_appSettings.AllowSiteWideTokenRefresh)
                {
                    // STEP 1: Validate JWT Token 
                    var jwtValidationResult = await ValidateAuthTokenAsync();

                    if (jwtValidationResult.IsValid && jwtValidationResult.Message == "Token Expired")
                    {
                        // check if there's an user with the refresh token's userId
                        var user = await _userManager.FindByNameAsync(model.UserName);

                        // also check if user is not admin / using admin cookie
                        if (user == null || user.UserRole == "Administrator")
                            return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
                        // check if the received refreshToken exists for the given clientId
                        var rt = _db.Tokens.FirstOrDefault(t =>
                                t.ClientId == _appSettings.ClientId
                                && t.UserId == user.Id);

                        if (rt == null)
                            return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);

                        // check if refresh token is expired
                        if (rt.ExpiryTime < DateTime.UtcNow)
                        {
                            _cookieSvc.DeleteCookie("access_token");
                            _cookieSvc.DeleteCookie("refreshToken");
                            _cookieSvc.DeleteCookie("loginStatus");
                            _cookieSvc.DeleteCookie("username");
                            _cookieSvc.DeleteCookie("userRole");
                            return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
                        }
                        /* Get the Data protection service instance */
                        var protectorProvider = _provider.GetService<IDataProtectionProvider>();
                        /* Create a protector instance */
                        var protectorRt = protectorProvider.CreateProtector(rt.EncryptionKeyRt);
                        var unprotectedToken = protectorRt.Unprotect(_cookieSvc.Get("refreshToken"));
                        var decryptedToken = unprotectedToken.ToString();

                        if (rt.Value != decryptedToken)
                            return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);

                        user.RememberMe = model.RememberMe;
                        var accessToken = await CreateAccessToken(user);
                        var expireTime = accessToken.Expiration.Subtract(DateTime.UtcNow).TotalMinutes;
                        var refreshTokenExpireTime = accessToken.RefreshTokenExpiration.Subtract(DateTime.UtcNow).TotalMinutes;
                        // set cookie for jwt and refresh token
                        // Expiry time for cookie - When Refresh token expires all other cookies should expire
                        // therefor set all the cookie expiry time to refresh token expiry time
                        _cookieSvc.SetCookie("access_token", accessToken.Token.ToString(), Convert.ToInt32(refreshTokenExpireTime));
                        _cookieSvc.SetCookie("refreshToken", accessToken.RefreshToken, Convert.ToInt32(refreshTokenExpireTime));
                        _cookieSvc.SetCookie("loginStatus", "1", Convert.ToInt32(refreshTokenExpireTime), false, false);
                        _cookieSvc.SetCookie("username", user.UserName, Convert.ToInt32(refreshTokenExpireTime), false, false);
                        _cookieSvc.SetCookie("userRole", user.UserRole, Convert.ToInt32(refreshTokenExpireTime), false, false);
                        _cookieSvc.SetCookie("user_id", accessToken.UserId, Convert.ToInt32(refreshTokenExpireTime));
                        accessToken.Principal = validateToken;
                        return accessToken;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return CreateErrorResponseToken($"Error => {ex.Message}", HttpStatusCode.Unauthorized);
            }

            return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
        }

        private async Task<ResponseObject> ValidateAuthTokenAsync()
        {
            var response = new ResponseObject();
            try
            {
                var authToken = _cookieSvc.Get("access_token");
                var userName = _cookieSvc.Get("username");

                if (!string.IsNullOrEmpty(authToken))
                {
                    var user = await _userManager.FindByNameAsync(userName);

                    if (user != null)
                    {
                        var userOldToken = await _db.Tokens.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();

                        if (userOldToken != null)
                        {
                            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
                            var layerOneUnProtector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                            var unprotectedTokenLayerOne = layerOneUnProtector.Unprotect(authToken);
                            var protectorJwt = protectorProvider.CreateProtector(userOldToken.EncryptionKeyJwt);
                            unProtectedToken = protectorJwt.Unprotect(unprotectedTokenLayerOne);
                            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

                            handler = new JwtSecurityTokenHandler();

                            validationParameters = new TokenValidationParameters
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

                            validateToken = handler.ValidateToken(unProtectedToken, validationParameters, out var securityToken);

                            /* This is called pattern matching => is */
                            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                    StringComparison.InvariantCultureIgnoreCase))
                            {
                                response.IsValid = false;
                                response.Message = "Token Invalid";
                                return response;
                            }

                            if (UserRoles.Contains(user.UserRole))
                            {
                                var decryptedUsername = validateToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                                if (decryptedUsername == userName)
                                {
                                    response.IsValid = true;
                                    response.Message = "Token Valid";
                                    return response;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(SecurityTokenExpiredException))
                {
                    if (_appSettings.AllowSiteWideTokenRefresh)
                    {
                        validationParameters.ValidateLifetime = false;
                        validateToken = handler.ValidateToken(unProtectedToken, validationParameters, out var securityToken);

                        /* This is called pattern matching => is */
                        if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                StringComparison.InvariantCultureIgnoreCase))
                        {
                            response.IsValid = false;
                            response.Message = "Token Invalid";
                            return response;
                        }

                        response.IsValid = true;
                        response.Message = "Token Expired";
                        return response;
                    }
                }

                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            response.IsValid = false;
            response.Message = "Token Invalid";
            return response;
        }

        private async Task<TokenResponseModel> GenerateNewToken(ApplicationUser user, LoginViewModel model)
        {
            // Create a key to encrypt the JWT 
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            // Get user role => check if user is admin
            var roles = await _userManager.GetRolesAsync(user);

            // Creating JWT token
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName), // Sub - Identifies principal that issued the JWT
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Jti - Unique identifier of the token
                        new Claim(ClaimTypes.NameIdentifier, user.Id), // Unique Identifier of the user
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()), // Role of the user
                        new Claim("LoggedOn", DateTime.Now.ToString(CultureInfo.InvariantCulture)), // Time When Created
                        new Claim("PlayerId", user.PlayerId.ToString())
                 }),

                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _appSettings.Site, // Issuer - Identifies principal that issued the JWT.
                Audience = _appSettings.Audience, // Audience - Identifies the recipients that the JWT is intended for.
                Expires = (string.Equals(roles.FirstOrDefault(), "Administrator", StringComparison.CurrentCultureIgnoreCase)) ? DateTime.UtcNow.AddMinutes(10080) : DateTime.UtcNow.AddMinutes(Convert.ToDouble(_appSettings.ExpireTime))
            };

            /* Create the unique encryption key for token - 2nd layer protection */
            var encryptionKeyRt = Guid.NewGuid().ToString();
            var encryptionKeyJwt = Guid.NewGuid().ToString();

            /* Get the Data protection service instance */
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();

            /* Create a protector instance */
            var protectorJwt = protectorProvider.CreateProtector(encryptionKeyJwt);

            /* Generate Token and Protect the user token */
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            /* Create and update the token table */
            TokenModel newRtoken = new TokenModel();

            /* Create refresh token instance */
            newRtoken = CreateRefreshToken(_appSettings.ClientId, user.Id, Convert.ToInt32(_appSettings.RtExpireTime));

            /* assign the tne JWT encryption key */
            newRtoken.EncryptionKeyJwt = encryptionKeyJwt;

            newRtoken.EncryptionKeyRt = encryptionKeyRt;

            /* Add Refresh Token with Encryption Key for JWT to DB */
            try
            {
                // First we need to check if the user has already logged in and has tokens in DB
                var rt = _db.Tokens
                    .FirstOrDefault(t => t.UserId == user.Id);

                if (rt != null)
                {
                    // invalidate the old refresh token (by deleting it)
                    _db.Tokens.Remove(rt);

                    // add the new refresh token
                    _db.Tokens.Add(newRtoken);

                }
                else
                    await _db.Tokens.AddAsync(newRtoken);

                // persist changes in the DB
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            // Return Response containing encrypted token
            var protectorRt = protectorProvider.CreateProtector(encryptionKeyRt);
            var layerOneProtector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);

            var encAuthToken = new TokenResponseModel
            {
                Token = encryptedToken,
                Expiration = token.ValidTo,
                RefreshToken = protectorRt.Protect(newRtoken.Value),
                Role = roles.FirstOrDefault(),
                Username = user.UserName,
                UserId = layerOneProtector.Protect(user.Id),
                PlayerId = user.PlayerId,
                ResponseInfo = CreateResponse("Auth Token Created", HttpStatusCode.OK)
            };

            return encAuthToken;
        }

        private static TokenResponseModel CreateErrorResponseToken(string errorMessage, HttpStatusCode statusCode)
        {
            var errorToken = new TokenResponseModel
            {
                Token = null,
                Username = null,
                PlayerId = 0,
                Role = null,
                RefreshTokenExpiration = DateTime.Now,
                RefreshToken = null,
                Expiration = DateTime.Now,
                ResponseInfo = CreateResponse(errorMessage, statusCode)
            };

            return errorToken;
        }

        private static ResponseStatusInfoModel CreateResponse(string errorMessage, HttpStatusCode statusCode)
        {
            var responseStatusInfo = new ResponseStatusInfoModel
            {
                Message = errorMessage,
                StatusCode = statusCode
            };

            return responseStatusInfo;
        }

        private static TokenModel CreateRefreshToken(string clientId, string userId, int expireTime)
        {

            return new TokenModel()
            {
                ClientId = clientId,
                UserId = userId,
                Value = Guid.NewGuid().ToString("N"),
                CreatedDate = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(expireTime),
                EncryptionKeyRt = "",
                EncryptionKeyJwt = ""
            };
        }

        private string GetLoggedInUserId()
        {
            try
            {
                var protectorProvider = _provider.GetService<IDataProtectionProvider>();
                var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                var unprotectUserId = protector.Unprotect(_cookieSvc.Get("user_id"));
                return unprotectUserId;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while decrypting user Id  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return null;

        }

        public Task<ApplicationUser> GetUserDetails(string playerId)
        {
            var result = _db.ApplicationUsers.SingleOrDefaultAsync(_ => _.PlayerId == int.Parse(playerId));
            return result;
        }
    }
}
