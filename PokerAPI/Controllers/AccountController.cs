using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AuthService;
using PokerAPI.Extensions;
//using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelService;
using Serilog;
using UserService;
using System.Net.NetworkInformation;
using ModelService.UserUpdateViewModel;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AuthService
{
    [ApiController]
    [Route("api/[controller]")]
    //[AutoValidateAntiforgeryToken]
    //[ValidateAntiForgeryToken]
    public class AccountController : ControllerBase
    {

        private readonly IUserSvc _userSvc;
        //private readonly IEmailSvc _emailSvc;
        private readonly IAuthSvc _authSvc;
        private readonly IWebHostEnvironment _hostingEnvironment;
        //private static HttpContextAccessor _httpContextAccessor;
        readonly string[] _cookiesToDelete = { "loginStatus", "access_token", "userRole", "username", "refreshToken" };

        public AccountController(IUserSvc userSvc, 
                                //IEmailSvc emailSvc, 
                                IAuthSvc authSvc,
                                IWebHostEnvironment hostingEnvironment
                                //HttpContextAccessor httpContextAccessor
                                )
        {
            _hostingEnvironment = hostingEnvironment;
            _userSvc = userSvc;
            //_emailSvc = emailSvc;
            _authSvc = authSvc;
            //_httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var result = await _userSvc.RegisterUserAsync(model);

            if (result.Message.Equals("Success") && result.IsValid)
            {
                // Sending Confirmation Email
                //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { UserId = result.Data["User"].Id, Code = result.Data["Code"] }, protocol: HttpContext.Request.Scheme);
                //RedirectToAction("ConfirmEmail", "Account", new { UserId = result.Data["User"].Id, Code = result.Data["Code"] });
                //await _emailSvc.SendEmailAsync(
                //    result.Data["User"].Email,
                //    "Thank you for Registration!",
                //    callbackUrl,
                //    "EmailConfirmation.html");

                Log.Information($"New User Created => {result.Data["User"].UserName}");

                return Ok(new { username = result.Data["User"].UserName, email = result.Data["User"].Email, status = 1, message = "Registration Successful" });
            }
            return BadRequest(new JsonResult(result.Data));
        }

        [HttpPost("Auth")]
        [AllowAnonymous]
        public async Task<IActionResult> Auth([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            //var ip = _httpContextAccessor.HttpContext.
            //foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
            //{
            //    item.
            //}
            
            try
            {
                var jwtToken = await _authSvc.Auth(model);

                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return Unauthorized(new { LoginError = jwtToken.ResponseInfo.Message });
                }
                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.InternalServerError)
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.BadRequest)
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return BadRequest(new { LoginError = jwtToken.ResponseInfo.Message });
                }

                return Ok(jwtToken);
            }

            //if (!jwtToken.TwoFactorLoginOn) 
            //return Ok(jwtToken);

            // Update the Response Message
            //jwtToken.ResponseInfo.Message = "Auth Code Required";

            //var twoFactorCodeModel = await _userSvc.GenerateTwoFactorCodeAsync(true, jwtToken.UserId);

            //if (twoFactorCodeModel == null)
            //{
            //    _authSvc.DeleteAllCookies(_cookiesToDelete);
            //    return BadRequest("Error");
            //}

            //if (twoFactorCodeModel.AuthCodeRequired)
            //{
            //    _authSvc.DeleteAllCookies(_cookiesToDelete);
            //    return Unauthorized(new
            //    {
            //        LoginError = jwtToken.ResponseInfo.Message,
            //        Expiry = twoFactorCodeModel.ExpiryDate,
            //        twoFactorToken = twoFactorCodeModel.Token,
            //        UserId = twoFactorCodeModel.UserId
            //    });
            //}

            //}
            catch (Exception ex)
            {
                Log.Error("An error occurred while authenticating {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return Unauthorized();
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authSvc.LogoutUserAsync();
            
            if (result)
                return Ok(new { Message = "Logout Successful" });
            
            return BadRequest(new { Message = "Invalid Request" });
        }

        [HttpGet("userdata/{playerId}")]
        public async Task<IActionResult> GetUserdata([FromRoute] string playerId)
        {
            var result = await _authSvc.GetUserDetails(playerId);
            result.PasswordHash = null;
            return Ok(result);
        }

        [HttpPost("updateFile/{userId}")]
        public async Task<IActionResult> UpdateFile([FromForm]IFormFile file, [FromRoute] string userId)
        {
            string uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

            if (file.Length > 0 || file != null)
            {
                 string filePath = Path.Combine(uploads, file.FileName);
                 using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                 {
                    await file.CopyToAsync(fileStream);
                    var user = await _userSvc.GetUserByIdAsync(userId);
                    user.ProfilePic = filePath;
                    await _userSvc.UpdateUserDetails(user);
                    
                 }
            }
            return Ok();
        }

        [HttpPut("updateUser/{userId}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string userId, [FromBody] UserUpdateVM user)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                var result = await _userSvc.UpdateUserDetails(userId, user);

                if (result != null)
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return Ok(result);
                }
            }

            _authSvc.DeleteAllCookies(_cookiesToDelete);
            return BadRequest(new { Message = "Failed" });
        }

        [HttpPut("updateUserAdmin/{userId}")]
        public async Task<IActionResult> UpdateUserFromAdmin([FromRoute] string userId, [FromBody] ApplicationUser user)
        {
            if (userId == user.Id)
            {
                var result = await _userSvc.UpdateUserDetails(user);

                if (result != null)  
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return Ok(result);
                }
            }

            _authSvc.DeleteAllCookies(_cookiesToDelete);
            return BadRequest(new { Message = "Failed" });
        }


        [HttpPost("ForgotPassword/{email}")]
        public async Task<IActionResult> ForgotPassword([FromRoute] string email)
        {
            if (ModelState.IsValid)
            {
                var result = await _userSvc.ForgotPassword(email);

                // Don't reveal that the user does not exist or is not confirmed
                if (!result.IsValid)
                    return Ok(new { Message = "Success" });

                RedirectToAction("ResetPassword", "Account", new { UserId = (string)result.Data["User"].Id, Code = (string)result.Data["Code"] });
                //var callbackUrl = GetAbsoluteUri().AbsoluteUri.
                //.AbsoluteUrl("/api/v1/Account/ResetPassword", new { userId = (string)result.Data["User"].Id, code = (string)result.Data["Code"] });
                //HttpContext.Curre("/api/v1/Account/ResetPassword", new { userId = (string)result.Data["User"].Id, code = (string)result.Data["Code"] });
                //await _emailSvc.SendEmailAsync(
                //    email,
                //    "Reset Password",
                //    callbackUrl,
                //    "ForgotPasswordConfirmation.html");
                return Ok(new { Message = "Success" });
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("We have Encountered an Error");
        }

        //private static Uri GetAbsoluteUri()
        //{
        //    var request = _httpContextAccessor.HttpContext.Request;
        //    UriBuilder uriBuilder = new UriBuilder();
        //    uriBuilder.Scheme = request.Scheme;
        //    uriBuilder.Host = request.Host.Host;
        //    uriBuilder.Path = request.Path.ToString();
        //    uriBuilder.Query = request.QueryString.ToString();
        //    return uriBuilder.Uri;
        //}

        // Similar methods for Url/AbsolutePath which internally call GetAbsoluteUri
        //public static string GetAbsoluteUrl() { }
        //public static string GetAbsolutePath() { }

        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            return RedirectToAction("ResetPassword", "Password", new ResetPasswordViewModel { Code = code });
        }

        //[HttpPost("[action]")]
        //public async Task<IActionResult> SendTwoFactorCode([FromBody] TwoFactorRequestModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    // First we need to check if this request is valid - We cannot depend on client side validation alone
        //    // Check the validity of TwoFactorToken & Session Expiry
        //    try
        //    {
        //        var result = await _userSvc.SendTwoFactorAsync(model);

        //        if (result.IsValid)
        //        {

        //            // Send code to the user via to their preferred provider.
        //            if (model.ProviderType.Equals("Email"))
        //            {
        //                var message = $"<h2>Your Two-Factor Authentication Code : {result.Code}</h2>";
        //                await _emailSvc.SendEmailAsync(
        //                    result.Email,
        //                    "Two-Factor Code",
        //                    message,
        //                    "TwoFactorAuthentication.html");


        //                return Ok(new { Message = "TwoFactorCode-Send" });
        //            }
        //            if (model.ProviderType.Equals("SMS"))
        //            {
        //                //TODO : Phase 2
        //                return BadRequest("SMS Service not implemented");
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        throw;
        //    }
        //    return Unauthorized(new { LoginError = "Two-Factor Fail" });
        //}

        [HttpPost("SessionExpiryNotification/{userId}")]
        public async Task<IActionResult> SessionExpiryNotification([FromRoute] string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                var result = await _userSvc.ExpireUserSessionAsync(userId);

                if (result.IsValid)
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return Ok(new { Message = "Success" });
                }
            }

            _authSvc.DeleteAllCookies(_cookiesToDelete);
            return BadRequest(new { Message = "Failed" });
        }

        //[HttpPost("[action]")]
        //public async Task<IActionResult> ValidateTwoFactor([FromBody] string code)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }
        //    try
        //    {
        //        var result = await _userSvc.ValidateTwoFactorCodeAsync(code);

        //        if (!result.IsValid)
        //        {
        //            return Unauthorized(new { LoginError = result.ResponseMessage.Message, AttemptsRemaining = result.Attempts });
        //        }

        //        var jwtToken = await _authSvc.GenerateNewToken();

        //        if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.Unauthorized) return Unauthorized(new { LoginError = jwtToken.ResponseInfo.Message });

        //        if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.OK)
        //        {
        //            return Ok(jwtToken);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    return Unauthorized(new { LoginError = "You request cannot be completed" });
        //}

    }
}
