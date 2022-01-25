using DataService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelService;
using ModelService.GameLogModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerAPI.Controllers.GameLogControllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoundWinnerLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoundWinnerLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RoundWinnerLog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoundWinner>>> GetRoundWinners()
        {
            try
            {
                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = await _context.RoundWinners.ToListAsync()
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

        // GET: api/RoundWinnerLog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoundWinner>> GetRoundWinner([FromRoute] int id)
        {
            try
            {
                var roundWinnerEntity = await _context.RoundWinners.FindAsync(id);

                if (roundWinnerEntity == null)
                    return NotFound();

                return Ok(new ResponseObject()
                {
                    IsValid = true,
                    Message = "Success",
                    Data = roundWinnerEntity
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

        // PUT: api/RoundWinnerLog/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoundWinner([FromRoute] int id, RoundWinner roundWinner)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != roundWinner.RoundWinnerLogId)
                return BadRequest();
            
            _context.Entry(roundWinner).State = EntityState.Modified;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RoundWinnerExists(id))
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

        // POST: api/RoundWinnerLog
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RoundWinner>> PostRoundWinner([FromBody] RoundWinner roundWinner)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.RoundWinners.Add(roundWinner);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetRoundWinner", new { id = roundWinner.RoundWinnerLogId }, roundWinner);
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

        // DELETE: api/RoundWinnerLog/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoundWinner(int id)
        {
            var roundWinner = await _context.RoundWinners.FindAsync(id);
            if (roundWinner == null)
                return NotFound();
            
            _context.RoundWinners.Remove(roundWinner);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoundWinnerExists(int id)
        {
            return _context.RoundWinners.Any(e => e.RoundWinnerLogId == id);
        }
    }
}
