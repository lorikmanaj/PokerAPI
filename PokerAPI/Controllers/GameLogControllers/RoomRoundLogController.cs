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
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RoomRoundLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomRoundLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RoomRoundLog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomRoundLog>>> GetRoomRoundLogs()
        {
            try
            {
                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = await _context.RoomRoundLogs.ToListAsync()
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

        // GET: api/RoomRoundLog/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomRoundLog([FromRoute] int id)
        {
            try
            {
                var roomRoundLogEntity = await _context.RoomRoundLogs.Where(_ => _.RoomId == id).ToListAsync();

                if (roomRoundLogEntity == null)
                    return NotFound();

                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = roomRoundLogEntity
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoundLogID([FromRoute] string id)
        {
            try
            {
                var roundLogEntity = await _context.RoundLogs.Include(_ => _.RoundCardsJson).Include(_ => _.RoundWinners).Where(_ => _.RoundLogId == id).ToListAsync();
                var userLog = await _context.RoundUsersLogs.Where(_ => _.RoundLogId == id).ToListAsync();
                var responseobject = new { roundLogEntity = roundLogEntity, userLog =  userLog};
                if (roundLogEntity == null)
                    return NotFound();

                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = responseobject
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


        // PUT: api/RoomRoundLog
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoomRoundLog([FromRoute] int id, [FromBody] RoomRoundLog roomRoundLog)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != roomRoundLog.RoomRoundLogId)
                return BadRequest();

            _context.Entry(roomRoundLog).State = EntityState.Modified;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RoomRoundLogExists(id))
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

        // POST: api/RoomLog
        [HttpPost]
        public async Task<IActionResult> PostRoomRoundLog([FromBody] RoomRoundLog roomRoundLog)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.RoomRoundLogs.Add(roomRoundLog);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetRoomRoundLog", new { id = roomRoundLog.RoomRoundLogId }, roomRoundLog);
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

        private bool RoomRoundLogExists(int id) { return _context.RoomRoundLogs.Any(e => e.RoomRoundLogId == id); }

    }
}