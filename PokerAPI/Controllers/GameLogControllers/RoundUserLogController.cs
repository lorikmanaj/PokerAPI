using DataService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelService.GameLogModels;
using Microsoft.EntityFrameworkCore;
using ModelService;

namespace PokerAPI.Controllers.GameLogControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoundUserLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoundUserLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RoundUserLog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoundUserLog>>> GetRoundUserLogs()
        {
            try
            {
                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = await _context.RoundUsersLogs.ToListAsync()
                });
            }
            catch (Exception ex) { return BadRequest(ex); }
        }

        // GET: api/RoundUserLog/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoundUserLog([FromRoute] int id)
        {
            try
            {
                var roundUserLogEntity = await _context.RoundUsersLogs.Where(_ => _.RoundUsersLogId == id).FirstOrDefaultAsync();

                if (roundUserLogEntity == null)
                    return NotFound();

                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = roundUserLogEntity
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseObject()
                {
                    IsValid = false,
                    Message = "Error",
                    Data = ex
                });
            }
        }

        // PUT: api/RoundUserLog/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoundUserLogLog([FromRoute] int id, [FromBody] RoundUserLog roundUserLog)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != roundUserLog.RoundUsersLogId)
                return BadRequest();

            _context.Entry(roundUserLog).State = EntityState.Modified;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RoundUserLogExists(id))
                    return NotFound();
                else
                    return BadRequest(new ResponseObject()
                    {
                        IsValid = false,
                        Message = "Error",
                        Data = ex
                    });
            }

            return NoContent();
        }

        // POST: api/RoundUserLog
        [HttpPost]
        public async Task<IActionResult> PostRoundUserLog([FromBody] RoundUserLog roundUserLog)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.RoundUsersLogs.Add(roundUserLog);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetRoundUserLog", new { id = roundUserLog.RoundUsersLogId }, roundUserLog);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseObject()
                {
                    IsValid = false,
                    Message = "Error",
                    Data = ex
                });
            }
        }

        private bool RoundUserLogExists(int id) { return _context.RoundUsersLogs.Any(e => e.RoundUsersLogId == id); }

    }
}