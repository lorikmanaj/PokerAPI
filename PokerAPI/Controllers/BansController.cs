using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataService;
using ModelService;

namespace PokerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BansController(ApplicationDbContext context)
        {
         
            _context = context;
        }

        // GET: api/Bans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ban>>> GetBanns()
        {
            return await _context.Banns.ToListAsync();
        }

        // GET: api/Bans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ban>> GetBan(string id)
        {
            var ban = await _context.Banns.FindAsync(id);

            if (ban == null)
                return NotFound();

            return ban;
        }

        // PUT: api/Bans/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBan(string id, Ban ban)
        {
            if (id != ban.UserId)
                return BadRequest();

            _context.Entry(ban).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BanExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Bans
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Ban>> PostBan(Ban ban)
        {
            _context.Banns.Add(ban);

            var bannedUser = await _context.ApplicationUsers.FindAsync(ban.UserId);
            bannedUser.IsActive = false;
            bannedUser.LockoutEnabled = true;

            _context.Entry(bannedUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BanExists(ban.UserId))
                    return Conflict();
                else
                    throw;
            }

            return CreatedAtAction("GetBan", new { id = ban.UserId }, ban);
        }

        // DELETE: api/Bans/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Ban>> DeleteBan(string id)
        {
            var ban = await _context.Banns.FindAsync(id);
            if (ban == null)
                return NotFound();

            _context.Banns.Remove(ban);
            await _context.SaveChangesAsync();

            return ban;
        }

        private bool BanExists(string id)
        {
            return _context.Banns.Any(e => e.UserId == id);
        }
    }
}
