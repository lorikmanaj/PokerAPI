using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelService;
using ModelService.GameLogModels;
using ModelService.GameModels;
using ModelService.RoundGameModel.Cards;
using PokerLogic.Models.Generic;
using PokerLogic.Models.SchemaModels.inGame;

namespace GameLogService
{
    public interface IGameLogSvc
    {
        Task<decimal> UpdateUserChips(double Chips, int playerId, bool isSitDown);
        Task<bool> CreateLogMessage(string className, string methodName, Exception exeption);
        Task<bool> CreateRoundLogs(RoomModel room, ResultSet resultSet, UserData[] newUserArray, Card[] tableCards);
        //Round Logs
        Room GetRoomRoomDetails(int roomId);
        Task<List<Room>> GetRoom();
        Task<ApplicationUser> GetUserByID(int id);

        Room AddUserInRoom(UserInRoom userInRoom, int roomID);

        Task<Room> RemoveUserInRoom(UserInRoom userInRoom, int roomID);
    }
}
