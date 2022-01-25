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
    public class RoundLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoundLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RoundLog
        [HttpGet]
        public async Task<ActionResult> GetRoundLogs()
        {
            try
            {
                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = await _context.RoundLogs.ToListAsync()
                });
            }
            catch (Exception ex) { return BadRequest(ex); }
        }

        // GET: api/RoundLog/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoundLog([FromRoute] string id)
        {
            try
            {
                var roundLogEntity = await _context.RoundLogs.Where(_ => _.RoundLogId == id).FirstOrDefaultAsync();

                if (roundLogEntity == null)
                    return NotFound();

                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = roundLogEntity
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

        // PUT: api/RoundLog/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoundLog([FromRoute] string id, [FromBody] RoundLog roundLog)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != roundLog.RoundLogId)
                return BadRequest();

            _context.Entry(roundLog).State = EntityState.Modified;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RoundLogExists(id))
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

        // POST: api/RoomLogs
        [HttpPost]
        public async Task<IActionResult> PostRoomLog([FromBody] RoundLog roundLog)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.RoundLogs.Add(roundLog);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetRoundLog", new { id = roundLog.RoundLogId }, roundLog);
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

        private bool RoundLogExists(string id) { return _context.RoundLogs.Any(e => e.RoundLogId == id); }

    }
}