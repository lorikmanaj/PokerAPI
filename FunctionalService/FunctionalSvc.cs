using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ModelService;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalService
{
    public class FunctionalSvc : IFunctionalSvc
    {
        private readonly AdminUserOptions _adminUserOptions;
        private readonly AppUserOptions _appUserOptions;
        private readonly UserManager<ApplicationUser> _userManager;

        public FunctionalSvc(IOptions<AppUserOptions> appUserOptions,
            IOptions<AdminUserOptions> adminUserOptions,
            UserManager<ApplicationUser> userManager)
        {
            _adminUserOptions = adminUserOptions.Value;
            _appUserOptions = appUserOptions.Value;
            _userManager = userManager;
        }

        public async Task CreateDefaultAdminUser()
        {
            try
            {
                var adminUser = new ApplicationUser
                {
                    Email = _adminUserOptions.Email,
                    UserName = _adminUserOptions.Username,
                    FirstName = _adminUserOptions.Firstname,
                    MiddleName = "TA",
                    LastName = _adminUserOptions.Lastname,
                    DisplayName = "Admin",
                    UserEmail = _adminUserOptions.Email,
                    Chips = 0,
                    Gender = "M",
                    Notes = "Default Admin",
                    ProfilePic = GetDefaultProfilePic(),
                    Birthday = "1997-06-27",
                    IsProfileComplete = true,
                    Terms = true,
                    UserRole = "Administrator",
                    AccountCreatedOn = DateTime.Now,
                    RememberMe = true,
                    IsActive = true,
                    BadLogins = 0,
                    ValidationCode = "Validated",
                    Validated = true,
                    EmailConfirmed = true,
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false
                };

                var result = await _userManager.CreateAsync(adminUser, _adminUserOptions.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Administrator");
                    Log.Information("Admin User Created {UserName}", adminUser.UserName);
                }
                else
                {
                    var errorString = string.Join(",", result.Errors);
                    Log.Error("Error while creating user {Error}", errorString);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }

        public async Task CreateDefaultUser()
        {
            try
            {
                var appUser = new ApplicationUser
                {
                    FirstName = _appUserOptions.Firstname,
                    MiddleName = "TU",
                    LastName = _appUserOptions.Lastname,
                    DisplayName = "Test User",
                    UserEmail = _appUserOptions.Email,
                    Chips = 1000,
                    Gender = "F",
                    Notes = "Test User",
                    Birthday = "1997-06-27",
                    IsProfileComplete = true,
                    Terms = true,
                    UserRole = "Player",
                    AccountCreatedOn = DateTime.Now,
                    RememberMe = true,
                    IsActive = true,
                    BadLogins = 0,
                    ValidationCode = "Validated",
                    Validated = true,
                    Email = _appUserOptions.Email,
                    UserName = _appUserOptions.Username,
                    EmailConfirmed = true,
                    ProfilePic = GetDefaultProfilePic(),
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false
                };

                var result = await _userManager.CreateAsync(appUser, _appUserOptions.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(appUser, "Player");
                    Log.Information("Player User Created {UserName}", appUser.UserName);
                }
                else
                {
                    var errorString = string.Join(",", result.Errors);
                    Log.Error("Error while creating user {Error}", errorString);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }

        private string GetDefaultProfilePic()
        {
            return string.Empty;
        }
    }
}
