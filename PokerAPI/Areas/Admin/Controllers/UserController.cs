using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelService;

namespace PokerAPI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public UserController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
        {
            var result =  await _dbContext.ApplicationUsers.Where(_ => _.UserRole == "Player").ToListAsync();
            foreach (var item in result)
            {
                item.PasswordHash = null;
            }
            return Ok(result);
        }

    }
}
