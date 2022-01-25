using DataService;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerAPI.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly ApplicationDbContext _db;
        public LobbyHub(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddUsersInLobbyToGroup(string groupName)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            //await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            //await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }
    }
}
