using System;
using System.Threading.Tasks;
using ActivityService;
using CookieService;
using DataService;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using ModelService;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net;
using Microsoft.Extensions.Configuration;
using ModelService.UserUpdateViewModel;

namespace UserService
{
    public class UserSvc : IUserSvc
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _db;
        private readonly ICookieSvc _cookieSvc;
        private readonly IActivitySvc _activitySvc;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly IServiceProvider _provider;
        //private readonly IHttpContextAccessor _httpContextAccessor;
        public IConfiguration Configuration { get; }

        public UserSvc(
                    UserManager<ApplicationUser> userManager,
                    IWebHostEnvironment env,
                    ApplicationDbContext db,
                    ICookieSvc cookieSvc,
                    IActivitySvc activitySvc,
                    IServiceProvider provider,
                    IOptions<DataProtectionKeys> dataProtectionKeys,
                    //IHttpContextAccessor httpContextAccessor,
                    IConfiguration configuration)
        {
            _userManager = userManager;
            _env = env;
            _db = db;
            _cookieSvc = cookieSvc;
            _activitySvc = activitySvc;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _provider = provider;
            //_httpContextAccessor = httpContextAccessor;
            Configuration = configuration;
        }

        public async Task<ResponseObject> RegisterUserAsync(RegisterViewModel model)
        {
            //all the errors related to registration
            var errorList = new List<string>();

            ResponseObject responseObject = new ResponseObject();

            try
            {
                var defaultProfilePicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}default{Path.DirectorySeparatorChar}profile.jpeg";
                
                var profPicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}";
                
                var extension = ".jpeg";
                var filename = DateTime.Now.ToString("yymmssfff");
                var path = Path.Combine(profPicPath, filename) + extension;
                var dbImagePath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}", filename) + extension;
                
                File.Copy(defaultProfilePicPath, path);

                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Username,
                    UserRole = "Player",
                    PhoneNumber = model.Phone,
                    FirstName = model.Firstname,
                    LastName = model.Lastname,
                    Gender = model.Gender,
                    Terms = model.Terms,
                    IsProfileComplete = false,
                    Birthday = model.Dob,
                    ProfilePic = defaultProfilePicPath,//dbImagePath,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true,
                    Validated = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //await _userManager.AddToRoleAsync(user, "Player");
                    var userRole = new IdentityUserRole<string>() { UserId = user.Id, RoleId = "2" };
                    await _db.UserRoles.AddAsync(userRole);
                    await _db.SaveChangesAsync();
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var dynamicProperties = new Dictionary<string, object> { ["Code"] = code, ["User"] = user };

                    responseObject.IsValid = true;
                    responseObject.Message = "Success";

                    responseObject.Data = dynamicProperties;
                    return responseObject;
                }

                foreach (var error in result.Errors)
                    errorList.Add(error.Description);

                responseObject.IsValid = false;
                responseObject.Message = "Failed";
                responseObject.Data = errorList;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while registering new user  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return responseObject;
        }

        public async Task<ProfileModel> GetUserProfileByIdAsync(string userId)
        {
            ProfileModel userProfile = new ProfileModel();

            var loggedInUserId = GetLoggedInUserId();

            var user = await _userManager.FindByIdAsync(loggedInUserId);

            if (user == null || user.Id != userId) return null;

            try
            {
                userProfile = new ProfileModel()
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    Phone = user.PhoneNumber,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    Displayname = user.DisplayName,
                    Firstname = user.FirstName,
                    Middlename = user.MiddleName,
                    Lastname = user.LastName,
                    IsEmailVerified = user.EmailConfirmed,
                    IsTermsAccepted = user.Terms,
                    ProfilePic = user.ProfilePic,
                    UserRole = user.UserRole,
                    IsAccountLocked = user.LockoutEnabled,
                    Activities = new List<ActivityModel>(_db.Activities.Where(x => x.UserId == user.Id)).OrderByDescending(o => o.Date).Take(20).ToList()
                };
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userProfile;
        }

        public async Task<ProfileModel> GetUserProfileByUsernameAsync(string username)
        {
            var userProfile = new ProfileModel();

            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);
                if (user == null || user.UserName != username) return null;

                userProfile = new ProfileModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    Phone = user.PhoneNumber,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    Displayname = user.DisplayName,
                    Firstname = user.FirstName,
                    Middlename = user.MiddleName,
                    Lastname = user.LastName,
                    IsEmailVerified = user.EmailConfirmed,
                    IsTermsAccepted = user.Terms,
                    ProfilePic = user.ProfilePic,
                    UserRole = user.UserRole,
                    IsAccountLocked = user.LockoutEnabled,
                    Activities = new List<ActivityModel>(_db.Activities.Where(x => x.UserId == user.Id)).OrderByDescending(o => o.Date).Take(20).ToList()
                };

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userProfile;
        }

        public async Task<ProfileModel> GetUserProfileByEmailAsync(string email)
        {
            var userProfile = new ProfileModel();

            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user == null || user.Email != email) return null;

                userProfile = new ProfileModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    Phone = user.PhoneNumber,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    Displayname = user.DisplayName,
                    Firstname = user.FirstName,
                    Middlename = user.MiddleName,
                    Lastname = user.LastName,
                    IsEmailVerified = user.EmailConfirmed,
                    IsTermsAccepted = user.Terms,
                    ProfilePic = user.ProfilePic,
                    UserRole = user.UserRole,
                    IsAccountLocked = user.LockoutEnabled,
                    Activities = new List<ActivityModel>(_db.Activities.Where(x => x.UserId == user.Id)).OrderByDescending(o => o.Date).Take(20).ToList()
                };

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return userProfile;
        }

        public async Task<bool> CheckPasswordAsync(ProfileModel model, string password)
        {
            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user.UserName != _cookieSvc.Get("username") ||
                    user.UserName != model.Username)
                    return false;

                if (!await _userManager.CheckPasswordAsync(user, password))
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateProfileAsync(IFormCollection formData)
        {
            var loggedInUserId = GetLoggedInUserId();
            var user = await _userManager.FindByIdAsync(loggedInUserId);

            if (user == null) return false;

            if (user.UserName != _cookieSvc.Get("username") ||
                user.UserName != formData["username"].ToString() ||
                user.Email != formData["email"].ToString())
                return false;

            try
            {
                ActivityModel activityModel = new ActivityModel { UserId = user.Id };
                await UpdateProfilePicAsync(formData, user);

                user.FirstName = formData["firstname"];
                user.Birthday = formData["birthdate"];
                user.LastName = formData["lastname"];
                user.MiddleName = formData["middlename"];
                user.DisplayName = formData["displayname"];
                user.Gender = formData["gender"];
                
                await _userManager.UpdateAsync(user);

                activityModel.Date = DateTime.UtcNow;
                activityModel.IpAddress = _cookieSvc.GetUserIP();
                activityModel.Location = _cookieSvc.GetUserCountry();
                activityModel.OperatingSystem = _cookieSvc.GetUserOS();
                activityModel.Type = "Profile update successful";
                activityModel.Icon = "fas fa-thumbs-up";
                activityModel.Color = "success";
                await _activitySvc.AddUserActivity(activityModel);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while updating profile {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return false;
        }

        public async Task<bool> AddUserActivity(ActivityModel model)
        {
            try
            {
                await _activitySvc.AddUserActivity(model);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return false;
        }

        public async Task<bool> ChangePasswordAsync(ProfileModel model, string newPassword)
        {
            bool result;
            try
            {
                ActivityModel activityModel = new ActivityModel
                {
                    Date = DateTime.UtcNow,
                    IpAddress = _cookieSvc.GetUserIP(),
                    Location = _cookieSvc.GetUserCountry(),
                    OperatingSystem = _cookieSvc.GetUserOS()
                };

                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user != null)
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);
                    var updateResult = await _userManager.UpdateAsync(user);
                    result = updateResult.Succeeded;
                    activityModel.UserId = user.Id;
                    activityModel.Type = "Password Changed successful";
                    activityModel.Icon = "fas fa-thumbs-up";
                    activityModel.Color = "success";
                    await _activitySvc.AddUserActivity(activityModel);
                }
                else
                    result = false;
            }
            catch (Exception ex)
            {
                result = false;
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return result;
        }

        public async Task<List<ActivityModel>> GetUserActivity(string username)
        {
            List<ActivityModel> userActivities = new List<ActivityModel>();

            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user == null || user.UserName != username) return null;

                userActivities = await _db.Activities.Where(x => x.UserId == user.Id).OrderByDescending(o => o.Date).Take(20).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userActivities;
        }

        public async Task<ResponseObject> ForgotPassword(string email)
        {
            ResponseObject responseObject = new ResponseObject();

            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    responseObject.Message = "Failed";
                    responseObject.IsValid = false;
                    responseObject.Data = null;
                    return responseObject;
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                responseObject.Message = "Success";
                responseObject.IsValid = true;
                var dynamicProperties = new Dictionary<string, object> { ["Code"] = code, ["User"] = user };
                responseObject.Data = dynamicProperties;
                return responseObject;

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred at ForgotPassword {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                responseObject.Message = "Error";
                responseObject.IsValid = false;
                responseObject.Data = null;
                return responseObject;
            }
        }

        public async Task<ResponseObject> ResetPassword(ResetPasswordViewModel model)
        {
            ResponseObject responseObject = new ResponseObject();
            ActivityModel activityModel = new ActivityModel
            {
                Date = DateTime.UtcNow,
                IpAddress = _cookieSvc.GetUserIP(),
                Location = _cookieSvc.GetUserCountry(),
                OperatingSystem = _cookieSvc.GetUserOS()
            };

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    responseObject.Message = "Failed";
                    responseObject.IsValid = false;
                    responseObject.Data = null;
                    return responseObject;
                }
                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

                if (result.Succeeded)
                {
                    activityModel.UserId = user.Id;
                    activityModel.Type = "Password Changed successful";
                    activityModel.Icon = "fas fa-key";
                    activityModel.Color = "warning";
                    await _activitySvc.AddUserActivity(activityModel);
                    responseObject.Message = "Success";
                    responseObject.IsValid = true;
                    responseObject.Data = null;
                    return responseObject;
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred at ForgotPassword {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            responseObject.Message = "Failed";
            responseObject.IsValid = false;
            responseObject.Data = null;
            return responseObject;
        }

        public async Task<ResponseObject> ExpireUserSessionAsync(string userId)
        {
            ResponseObject responseObject = new ResponseObject
            {
                Message = "Failed",
                IsValid = false,
                Data = null
            };

            ActivityModel activityModel = new ActivityModel
            {
                Date = DateTime.UtcNow,
                IpAddress = _cookieSvc.GetUserIP(),
                Location = _cookieSvc.GetUserCountry(),
                OperatingSystem = _cookieSvc.GetUserOS()
            };

            try
            {
                var decryptedUserId = DecryptData(userId, _dataProtectionKeys.ApplicationUserKey).ToString();

                var user = await _userManager.FindByIdAsync(decryptedUserId);

                if (user != null)
                {
                    var result = await DeleteUserTokensAsync(user.Id);

                    if (result)
                    {
                        activityModel.UserId = user.Id;
                        activityModel.Type = "Session Expired";
                        activityModel.Icon = "fas fa-clock";
                        activityModel.Color = "danger";
                        await _activitySvc.AddUserActivity(activityModel);
                        responseObject.Message = "Success";
                        responseObject.IsValid = true;
                        responseObject.Data = null;

                        return responseObject;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return responseObject;
        }

        private T DecryptData<T>(T encryptedData, string key)
        {
            dynamic decryptedData = null;
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectorProvider.CreateProtector(key);

            if (encryptedData != null)
                decryptedData = protector.Unprotect(encryptedData.ToString());

            return decryptedData;
        }

        private string EncryptData<T>(string data, string key)
        {
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectorProvider.CreateProtector(key);
            if (data != null)
            {
                var encryptedData = protector.Protect(data);
                return encryptedData;
            }
            return null;
        }

        private string GetLoggedInUserId()
        {
            try
            {
                var protectorProvider = _provider.GetService<IDataProtectionProvider>();
                var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                var unprotectUserId = protector.Unprotect(_cookieSvc.Get("user_id")).ToString();
                return unprotectUserId;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while decrypting user Id  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return null;
        }

        private async Task<ApplicationUser> UpdateProfilePicAsync(IFormCollection formData, ApplicationUser user)
        {
            var oldProfilePic = new string[1];
            oldProfilePic[0] = Path.Combine(_env.WebRootPath + user.ProfilePic);

            var profPicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}";

            if (formData.Files.Count <= 0) return user;

            var extension = Path.GetExtension(formData.Files[0].FileName);
            var filename = DateTime.Now.ToString("yymmssfff");
            var path = Path.Combine(profPicPath, filename) + extension;
            var dbImagePath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}", filename) + extension;

            user.ProfilePic = dbImagePath;

            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await formData.Files[0].CopyToAsync(stream);
            }

            if (!System.IO.File.Exists(oldProfilePic[0])) return user;

            System.IO.File.SetAttributes(oldProfilePic[0], FileAttributes.Normal);
            System.IO.File.Delete(oldProfilePic[0]);

            return user;
        }

        private async Task<bool> DeleteUserTokensAsync(string userId)
        {
            await using var dbContextTransaction = await _db.Database.BeginTransactionAsync();
            var result = false;

            try
            {
                var oldJwtoken = await _db.Tokens.Where(x => x.UserId == userId).ToListAsync();

                if (oldJwtoken.Count > 0)
                    _db.Tokens.RemoveRange(oldJwtoken);

                await _db.SaveChangesAsync();
                await dbContextTransaction.CommitAsync();
                result = true;
                return result;
            }
            catch (Exception ex)
            {
                await dbContextTransaction.RollbackAsync();

                Log.Error("An error occurred {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return result;
        }


        public async Task<ApplicationUser> UpdateUserDetails(string userId, UserUpdateVM user)
        {
            try
            {
                var result = await _db.ApplicationUsers.FindAsync(userId);
                switch (user.key)
                {
                    case "username":
                        result.UserName = user.value;
                        break;
                    case "password":
                        result.PasswordHash = _userManager.PasswordHasher.HashPassword(result, user.value);
                        break;
                    case "firstname":
                        result.FirstName = user.value;
                        break;
                    case "lastname":
                        result.LastName = user.value;
                        break;
                    case "email":
                        result.Email = user.value;
                        break;
                    case "phonenumber":
                        result.PhoneNumber = user.value;
                        break;
                }
                _db.ApplicationUsers.Update(result).Property(x => x.PlayerId).IsModified = false;
                await _db.SaveChangesAsync();
                result.PasswordHash = "";
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<ApplicationUser> UpdateUserDetails(ApplicationUser user)
        {
            _db.ApplicationUsers.Update(user).Property(x => x.PlayerId).IsModified = false;
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.Id != userId) return null;
            else return user;
        }
    }
}
