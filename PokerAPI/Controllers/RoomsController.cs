using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataService;
using ModelService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using PokerAPI.Hubs;

namespace PokerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        protected readonly IHubContext<LobbyHub> _hubcontext;
        protected readonly IServiceProvider _serviceProvider;
        public RoomsController(ApplicationDbContext context, IHubContext<LobbyHub> hubcontext, IServiceProvider serviceProvider)
        {
            _context = context;
            _hubcontext = hubcontext;
            _serviceProvider = serviceProvider;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms.OrderBy(_ => _.RoomName).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoomByID(int id)
        {

            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound();

            return room;
        }
        //TEST ENVIROMENT TO BE CHANGED TO MIDDLEWARE
        [HttpGet]
        public async Task<bool> InsertUserToRoom()
        {
            var result = _context.Rooms.Find(1);
            var chatHub = (IHubContext<LobbyHub>)_serviceProvider.GetService(typeof(IHubContext<LobbyHub>));
            await chatHub.Clients.Group("Lobby").SendAsync("LobbyListener", result);
            return true;
        }

        [HttpPut("{roomId}")]
        public async Task<ActionResult<Room>> UpdateRoom([FromRoute]int roomId, [FromBody] Room room)
        {
            if(roomId != room.RoomId)
            {
                return BadRequest();
            }
            _context.Entry(room).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return room;
        }
    }
}
