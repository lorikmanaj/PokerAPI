using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CookieService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using ModelService;
using UserService;

namespace PokerAPI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "Admin")]
    [Route("admin/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly IUserSvc _userSvc;
        private readonly ICookieSvc _cookieSvc;
        private readonly IServiceProvider _provider;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly AppSettings _appSettings;
        //private readonly IDashboardSvc _dashboardSvc;
        //private readonly IWritableSvc<SiteWideSettings> _writableSiteWideSettings;
        //private AdminBaseViewModel _adminBaseViewModel;

        public HomeController(IUserSvc userSvc, ICookieSvc cookieSvc,
            IServiceProvider provider,
            IOptions<DataProtectionKeys> dataProtectionKeys, IOptions<AppSettings> appSettings)//,
            //IWritableSvc<SiteWideSettings> writableSiteWideSettings, IDashboardSvc dashboardSvc)
        {
            _userSvc = userSvc;
            _cookieSvc = cookieSvc;
            _provider = provider;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _appSettings = appSettings.Value;
            //_writableSiteWideSettings = writableSiteWideSettings;
            //_dashboardSvc = dashboardSvc;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
            var userProfile = await _userSvc.GetUserProfileByIdAsync(protector.Unprotect(_cookieSvc.Get("user_id")));
            var addUserModel = new AddUserModel();
            //var dashboard = await _dashboardSvc.GetDashboardData();

            var _adminBaseViewModel = new AdminBaseViewModel
            {
                Profile = userProfile,
                AddUser = addUserModel,
                AppSetting = _appSettings,
                //Dashboard = dashboard,
                //SiteWideSetting = _writableSiteWideSettings.Value
            };

            return View("Index", _adminBaseViewModel);
        }
    }
}
