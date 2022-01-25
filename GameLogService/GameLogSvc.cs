using ModelService.GameLogModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DataService;
using Microsoft.EntityFrameworkCore;
using ModelService.GameModels;
using PokerLogic.Models.SchemaModels.inGame;
using PokerLogic.Models.Generic;
using ModelService.RoundGameModel.Cards;
using System.Diagnostics;
using System.Linq;
using ModelService;
using Microsoft.Extensions.DependencyInjection;

namespace GameLogService
{
    public class GameLogSvc : IGameLogSvc
    {
        private readonly ApplicationDbContext _db;
        private IServiceScopeFactory serviceScopeFactory;

        public GameLogSvc(ApplicationDbContext db, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider)
        {
            _db = db;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        #region Add Methods
        //Create


        public async Task<bool> CreateRoundLogs(RoomModel room, ResultSet resultSet, UserData[] newUserArray, Card[] tableCards)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var roundLog = new RoundLog()
                        {
                            RoungLogStartDate = DateTime.Now,
                            RoungLogEndDate = DateTime.Now,
                        };
                        //var roundlogid = await CreateRoundLog(roundLog);
                        var roundlog = dbContext.RoundLogs.Add(roundLog).Entity;
                        var roomRoundLog = new RoomRoundLog
                        {
                            RoundLogId = roundlog.RoundLogId,
                            RoomId = room.roomId
                        };

                        dbContext.RoomRoundLogs.Add(roomRoundLog);

                        dbContext.CardModels.AddRange(CardsInTable(tableCards, roundlog.RoundLogId));


                        var roundWinner = RoundTableWinners(resultSet, newUserArray, roundlog.RoundLogId);
                        
                        dbContext.RoundWinners.AddRange(roundWinner);

                        var i = 0;
                        foreach (var item in newUserArray)
                        {
                            if (item != null)
                            {
                                var roundUser = new RoundUserLog();
                                if (room.roundModel.hands != null)
                                {
                                    roundUser = new RoundUserLog()
                                    {
                                        //UserLogId = userLogModel,
                                        FullName = item.dataBlock.UserName,
                                        PlayerId = item.dataBlock.PlayerId,
                                        RoundLogId = roundlog.RoundLogId,

                                        //UserPoints = (room.roundModel.hands[i] != null ? room.roundModel.hands[i].handPoints : 0),
                                        RoundUserCardsJson = CardsInTableToString(room.roundModel.playerFirstCards[i], room.roundModel.playerSecondCards[i]),
                                    };

                                }
                                else
                                {
                                    roundUser = new RoundUserLog()
                                    {
                                        //UserLogId = userLogModel,
                                        FullName = item.dataBlock.UserName,
                                        PlayerId = item.dataBlock.PlayerId,
                                        RoundLogId = roundlog.RoundLogId,
                                        RoundUserCardsJson = CardsInTableToString(room.roundModel.playerFirstCards[i], room.roundModel.playerSecondCards[i]),
                                    };
                                }

                                dbContext.RoundUsersLogs.Add(roundUser);
                            }
                            i++;
                        }
                        await dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }

                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        await CreateLogMessage("GameLogSvc", "CreateRoundLogs", ex);

                        Console.WriteLine("Error occurred.");
                    }
                }
            }

            return true;
        }

     
        public async Task<decimal> UpdateUserChips(double Chips, int playerId, bool isSitDown)
        {
            try
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    var userdb = dbContext.Users.ToList().Where(_ => _.PlayerId == playerId).FirstOrDefault();
                    
                    if(isSitDown)
                        userdb.Chips -= Convert.ToDecimal(Chips);
                    else
                        userdb.Chips += Convert.ToDecimal(Chips);
                    
                    if (userdb == null)
                        throw new Exception();

                    //_db.Entry(userdb).State = EntityState.Modified;

                    //dbContext.MyTable.Add(new MyObject { Id = "c6334a9e-4e7a-48bd-9b65-290c92b85f6f", Message = "Test bg thread" });
                    await dbContext.SaveChangesAsync();

                    return userdb.Chips;
                }
            }
            catch (Exception ex)
            {
                await CreateLogMessage("GameLogSvc", "UpdateUserChips", ex);
                return 0;
            }
        }

        public async Task<bool> CreateLogMessage(string className, string methodName, Exception exeption)
        {

            try
            {


                var st = new StackTrace(exeption, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                methodName = frame.GetMethod().Name;
                var loError = new LogError
                {
                    DateOccurred = DateTime.Now,
                    Message = exeption.Message,
                    Class = className,
                    Method = methodName,
                    Description = exeption.StackTrace,
                    Line = line.ToString()
                };

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    dbContext.LogErrors.Add(loError);
                    await dbContext.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

   
        public async Task<List<Room>> GetRoom()
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                try
                {
                    var rooms = await dbContext.Rooms.ToListAsync();
                    return rooms;
                }
                catch (Exception ex)
                {
                    await CreateLogMessage("GameLogSvc", "CreateRoundLogs", ex);
                }
            }
            return null;
        }

        public async Task<ModelService.ApplicationUser> GetUserByID(int id)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                try
                {
                    return await dbContext.Users.Where(_ => _.PlayerId == id).FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {

                    await CreateLogMessage("GameLogSvc", "CreateRoundLogs", ex);
                }
            }
            return null;
        }
        #endregion


        #region Utils

        private List<CardModel> CardsInTable(Card[] cards, string roundLogID)
        {
            List<CardModel> cards1 = new List<CardModel>();
            foreach (var item in cards)
            {
                CardModel cardModel = new CardModel();
                cardModel.Suit = item.suit.ordinal();
                cardModel.Value = item.value.NumericValue;
                cardModel.RoundLogId = roundLogID;
                cards1.Add(cardModel);

            }
            return cards1;
        }

        private string CardsInTableToString(Card firstCards, Card secondCard)
        {
            return firstCards.suit.innerEnumValue.ToString() + "  " + firstCards.value.NumericValue.ToString() + "  ,  " + secondCard.suit.innerEnumValue.ToString() + "  " + secondCard.value.NumericValue.ToString();
        }

        private List<RoundWinner> RoundTableWinners(ResultSet resultSet, UserData[] newArrayUsers, string roundLogID)
        {
            List<RoundWinner> roundWinners = new List<RoundWinner>();
            foreach (var item in resultSet.winners)
            {
                RoundWinner cardModel = new RoundWinner();
                cardModel.FullPot = item.fullPot;
                cardModel.PlayerId = newArrayUsers[item.position].dataBlock.PlayerId;
                cardModel.Points = item.points;
                cardModel.Pot = item.pot;
                cardModel.Reason = item.reason;
                cardModel.RoundLogId = roundLogID;
                cardModel.GameFeeCharge = item.GameFeeCharge;
                roundWinners.Add(cardModel);
            }

            return roundWinners;
        }

        public Room GetRoomRoomDetails(int roomId)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                try
                {
                    return dbContext.Rooms.Where(_ => _.RoomId == roomId).FirstOrDefault();
                }
                catch (Exception ex)
                {

                    CreateLogMessage("GameLogSvc", "CreateRoundLogs", ex);
                }
            }
            return null;
        }


        public Room AddUserInRoom(UserInRoom userInRoom,int roomID)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                try
                {
                    var userInRooms = dbContext.UsersInRooms.AsNoTracking().Where(_ => _.RoomId == roomID && _.UserId == userInRoom.UserId).FirstOrDefault();
                    if (userInRooms == null)
                        dbContext.UsersInRooms.Add(userInRoom);
                    else
                        dbContext.UsersInRooms.Update(userInRoom);

                    var room = dbContext.Rooms.Where(_ => _.RoomId == roomID).FirstOrDefault();
                    room.PlayersIngame += 1;

                    dbContext.SaveChanges();
        
                    return room;
                }
                catch (Exception ex)
                {

                   CreateLogMessage("GameLogSvc", "CreateRoundLogs", ex);
                }
            }
            return null;
        }


        public async Task<Room> RemoveUserInRoom(UserInRoom userInRoom, int roomID)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                try
                {
                    var userInRooms = dbContext.UsersInRooms.AsNoTracking().Where(_ => _.RoomId == roomID && _.UserId == userInRoom.UserId).FirstOrDefault();
                    if (userInRooms != null)
                        dbContext.UsersInRooms.Remove(userInRoom);

                    var room = dbContext.Rooms.Where(_ => _.RoomId == roomID).FirstOrDefault();
                    room.PlayersIngame -= 1;

                    dbContext.SaveChanges();

                    return room;
                }
                catch (Exception ex)
                {
                    await CreateLogMessage("GameLogSvc", "CreateRoundLogs", ex);
                }
            }
            return null;
        }

        #endregion
    }
}
