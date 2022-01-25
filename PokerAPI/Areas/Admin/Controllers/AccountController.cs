using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService;
using CookieService;
using DataService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using ModelService;
using Serilog;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace PokerAPI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/[controller]/[action]")]
    //[AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly IServiceProvider _provider;
        private readonly ApplicationDbContext _db;
        private readonly IAuthSvc _authSvc;
        private readonly ICookieSvc _cookieSvc;
        private const string AccessToken = "access_token";
        private const string User_Id = "user_id";
        readonly string[] cookiesToDelete = { "memberId", "rememberDevice", "user_id", "access_token" };//"twoFactorToken", 

        public AccountController(IOptions<AppSettings> appSettings,
            IServiceProvider provider,
            ApplicationDbContext db,
            IAuthSvc authSvc,
            ICookieSvc cookieSvc, IOptions<DataProtectionKeys> dataProtectionKeys)
        {
            _appSettings = appSettings.Value;
            _provider = provider;
            _db = db;
            _authSvc = authSvc;
            _cookieSvc = cookieSvc;
            _dataProtectionKeys = dataProtectionKeys.Value;
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> Login(string returnUrl = null)
        //{
        //    await Task.Delay(0);
        //    ViewData["ReturnUrl"] = returnUrl;
        //    try
        //    {
        //        // Check if user is already logged in 
        //        if (!Request.Cookies.ContainsKey(AccessToken) || !Request.Cookies.ContainsKey(User_Id))
        //            return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
        //            ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
        //    }

        //    return RedirectToAction("Index", "Home");
        //}

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            //ViewData["ReturnUrl"] = returnUrl;

            try
            {
                var jwtToken = await _authSvc.Auth(model);

                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return Unauthorized(new { LoginError = jwtToken.ResponseInfo.Message });
                }
                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(new { LoginError = jwtToken.ResponseInfo.Message });
                }

                return Ok(jwtToken);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while authenticating {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = _cookieSvc.Get("user_id");

                if (userId != null)
                {
                    var protectorProvider = _provider.GetService<IDataProtectionProvider>();
                    var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                    var unprotectedToken = protector.Unprotect(userId);

                    var rt = _db.Tokens.FirstOrDefault(t => t.UserId == unprotectedToken);

                    if (rt != null) _db.Tokens.Remove(rt);
                    await _db.SaveChangesAsync();

                    _cookieSvc.DeleteAllCookies(cookiesToDelete);
                }

            }
            catch (Exception ex)
            {
                _cookieSvc.DeleteAllCookies(cookiesToDelete);
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            Log.Information("User logged out.");
            return RedirectToLocal(null);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            // Preventing open redirect attack
            return Url.IsLocalUrl(returnUrl)
                ? (IActionResult)Redirect(returnUrl)
                : RedirectToAction(nameof(HomeController.Index), "Home");
        }

    }
}
