using ModelService;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Helpers
{
    public class DefaultDataHelper
    {
        private ApplicationDbContext _dbContext;
        public DefaultDataHelper(ApplicationDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task CreateDefaultRooms()
        {
            await Task.Delay(0);
            try
            {
                var lstRooms = new List<Room>();
                for (int start = 1; start < 51; start++)
                {
                    var room = new Room
                    {
                        RoomName = $"Room {start}",
                        AccessPassword = "SomePassword",
                        SecurityToken = "SecToken",
                        ServerIP = "Server IP",
                        GProto = "GProtocol",
                        MaxPlayers = 7,
                        Description = $"Description for Room number {start}",
                        MinCoinForAccess = start * 10,
                        RecoveryEmail = "RecoveryEmail",
                        BadLogins = 0,
                        NowConnected = true,
                        IsOfficial = false
                    };

                    lstRooms.Add(room);
                }

                var result = _dbContext.Rooms.AddRangeAsync(lstRooms).IsCompletedSuccessfully;
                await _dbContext.SaveChangesAsync();

                if (result)
                    Log.Information($"Default Rooms {lstRooms.Count} were created!");
                else
                    Log.Error("Error while creating default rooms.");
            }
            catch (Exception ex)
            {
                Log.Error("Error while creating default rooms {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }
    }
}
