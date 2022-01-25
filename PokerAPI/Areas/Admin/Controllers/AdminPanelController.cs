using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityService;
using DataService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelService;
using Serilog;

namespace PokerAPI.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminPanelController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IActivitySvc _activitySvc;

        public AdminPanelController(ApplicationDbContext db, IActivitySvc activitySvc)
        {
            _db = db;
            _activitySvc = activitySvc;
        }

        [HttpGet("UserProfiler")]
        public async Task<ActionResult<ResponseObject>> GetUserProfileData([FromForm] string userId)
        {
            try
            {
                var user = _db.Users.Where(_ => _.Id == userId).FirstOrDefault();
                var userActivities = await _activitySvc.GetUserActivities(userId);

                return new ResponseObject()
                {
                    Data = new ProfileModel(user, userActivities),
                    IsValid = true,
                    Message = "User Profile data and activities."
                };
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred fetching data {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest(ex);
            }
        }

        [HttpGet("Banns")]
        public async Task<ActionResult<ResponseObject>> GetBanns()
        {
            await Task.Delay(0);
            try
            {
                return new ResponseObject()
                {
                    Data = _db.Banns.ToList(),
                    IsValid = true,
                    Message = "Banns returned from DB"
                };
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred fetching data {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest(ex);
            }
        }

        [HttpGet("Users")]
        public async Task<ActionResult<ResponseObject>> GetUsers()
        {
            await Task.Delay(0);
            try 
            {
                return new ResponseObject()
                {
                    Data = _db.Users.ToList(),
                    IsValid = true,
                    Message = "Users returned from DB"
                };
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred fetching data {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest(ex);
            }
        }

        [HttpGet("Reports")]
        public async Task<ActionResult<ResponseObject>> GetReports()
        {
            await Task.Delay(0);
            try
            {
                return new ResponseObject()
                {
                    Data = _db.Reports.ToList(),
                    IsValid = true,
                    Message = "Reports returned from DB"
                };
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred fetching data {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest(ex);
            }
        }

        [HttpGet("Warnings")]
        public async Task<ActionResult<ResponseObject>> GetWarnings()
        {
            await Task.Delay(0);
            try
            {
                return new ResponseObject()
                {
                    Data = _db.Warnings.ToList(),
                    IsValid = true,
                    Message = "Warnings returned from DB"
                };
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred fetching data {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest(ex);
            }
        }

        [HttpGet("Activities")]
        public async Task<ActionResult<ResponseObject>> GetActivites()
        {
            await Task.Delay(0);
            try
            {
                return new ResponseObject()
                {
                    Data = _activitySvc.GetActivities().Result,
                    IsValid = true,
                    Message = "Activities returned from DB"
                };
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred fetching data {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest(ex);
            }
        }
    }
}
