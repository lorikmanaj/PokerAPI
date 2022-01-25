using DataService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ModelService;
using PokerLogic.Models.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelService.GameModels;
using SocketService.Utils;
using PokerLogic.Utils;
using PokerLogic.Models.SchemaModels.inGame;
using PokerLogic.Models;
using ModelService.RoundGameModel.Deck;
using ModelService.RoundGameModel.Cards;
using ModelService.RoundGameModel.Hands;
using PokerLogic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Newtonsoft.Json;
using PokerLogic.Models.Accesing;
using System.Timers;
using System.Threading;
using Microsoft.Extensions.Logging;
using GameLogService;
using ModelService.GameLogModels;
using HoldemHand;

namespace PokerAPI.Hubs
{
    public class RoomHub : Hub
    {

        private readonly IHubContext<RoomHub> _roomHubContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGameLogSvc _gameLogSvc;
        private EpprGameProto gameProtocol = new EpprGameProto();
        private readonly IHubContext<LobbyHub> _loobyHubcontext;

        private static ConcurrentDictionary<string, RoomModel> Rooms { get; } = new ConcurrentDictionary<string, RoomModel>();
        public RoomHub(ApplicationDbContext db, IHubContext<RoomHub> roomHubContext, IServiceProvider serviceProvider, IGameLogSvc gameLogSvc, IHubContext<LobbyHub> loobyHubcontext)
        {
            _serviceProvider = serviceProvider;
            _loobyHubcontext = loobyHubcontext;
            _roomHubContext = roomHubContext;
            _gameLogSvc = gameLogSvc;
            InitGameHandlerLst();

        }



        #region ConnectingHub

        // This method add player inside a room as spectator ! 
        public async Task EnterRoomAsync(string roomId, string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            var room = Rooms.TryGetValue(roomId, out var item);
            var userTemp = await GetCurrentUser(int.Parse(userId), ChallengeActions.LOGIN, UserDataStatus.PENDING);

            if (item.Users.Exists(_ => _.dataBlock.PlayerId == userTemp.dataBlock.PlayerId))
            {
                var user = item.Users.Find(_ => _.dataBlock.PlayerId == userTemp.dataBlock.PlayerId);
                user.sessID = userTemp.sessID;
                userTemp = user;
            }
            else
                item.Users.Add(userTemp);
            //REFRESH is only to test and to add in front how much coins user have in DB

            ingressFlow(userTemp, item);
            var data = new { chips = userTemp.dataBlock.Chips };
            await _roomHubContext.Clients.Clients(userTemp.sessID).SendAsync("RefreshChips", data);
        }

        //Request Deposit And Sit Down inside the Room...IF that position Is Free 
        public async Task RequestDeposit(string roomId, string userId, double chips, int position)
        {
            Rooms.TryGetValue(roomId.ToString(), out var room);
            var uD = room.Users.Where(_ => _.dataBlock.PlayerId == int.Parse(userId)).FirstOrDefault();

            ModelService.GameModels.SuccesDeposit succesDeposit = new ModelService.GameModels.SuccesDeposit
            {
                chips = chips,
                userID = uD.dataBlock.PlayerId.ToString()

            };
            //out.chips = dataResponse.depositedChips;  
            if (!Utils.isPlayerExistsInTablePosition(position, room, uD) && uD != null)
                if (uD.dataBlock.Chips >= decimal.Parse(chips.ToString()))
                {
                    await _roomHubContext.Clients.Clients(uD.sessID).SendAsync("GameController/successDeposit", succesDeposit);
                    await AddChipsToUser(userId, chips, Convert.ToInt64(uD.dataBlock.Chips), room, position);
                }
                else
                    await _roomHubContext.Clients.Clients(uD.sessID).SendAsync("GameController/invalidDeposit", succesDeposit);
            else
                await _roomHubContext.Clients.Clients(uD.sessID).SendAsync("GameController/rejectFullyfied", gameProtocol.getRejectFullyfiedSchema());


        }

        public void decisionInform(DecisionInform dI, string userId, string roomId)
        {
            Rooms.TryGetValue(roomId, out var room);

            bool finishedRound = processDecision(dI, userId, room);
            if (finishedRound)
                StartRoundWithTimer(room);
        }

        //Removed PLayer From Table but dont remove From hub because is Spectator yet
        public async Task RemovePlayerFromRoom(string roomId, string userId)
        {
            //GameRepo.GameRepo.RemoveUserFromRoom(roomId, userId)
            //await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            Rooms.TryGetValue(roomId, out var room);
            var player = Utils.getPlayerById(room, int.Parse(userId));
            room.leaveRequests.Add(player);
            int ppos = Utils.getPlyerPosition(room.PlayerArray, player);


            if (!room.inGame)
                await processLeaveRequests(room, false);
            else
            {
                if (room.roundModel != null)
                {
                    bool finishedRound = processDecision(new DecisionInform() { action = "fold", ammount = 0, position = ppos, isLeaving = true }, room.PlayerArray[ppos].dataBlock.PlayerId.ToString(), room);
                    await processLeaveRequests(room, false);
                    if (finishedRound)
                    {
                        if (Utils.countUsersCanPlay(room.PlayerArray) > 1)
                        {
                            StartRoundWithTimer(room);
                        }
                        else
                        {
                            StartRound(room);
                        }
                    }
                }
            }
        }

        //Removed PLayer From Table And from Spectator Mode
        public async Task RemovePlayerFromHub(string roomId, string userId)
        {
            Rooms.TryGetValue(roomId, out var room);
            room.leaveRequests.Add(Utils.getPlayerById(room, int.Parse(userId)));
            var user = room.Users.Find(_ => _.dataBlock.PlayerId == int.Parse(userId));

            await RemovePlayerFromRoom(roomId, userId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        }


        #endregion

        #region GameHandler

        public void ingressFlow(UserData userData, RoomModel room)
        {

            List<int> freeSpaces = new List<int>();
            for (int i = 0; i < room.maxPlayers; i++)
            {
                if (room.PlayerArray[i] != null)
                {
                    if (room.PlayerArray[i].dataBlock.PlayerId == userData.dataBlock.PlayerId)
                    {
                        userData.chips = room.PlayerArray[i].chips;
                        room.PlayerArray[i].sessID = userData.sessID;
                        if (room.roundModel != null)
                            Utils.changeSessIDInUserInGamer(ref room.roundModel.usersInGame, room.PlayerArray[i]);
                        dumpSnapshot(room, userData, i);
                        ingressSchema(i, userData, room);
                        return;
                    }
                }
                else
                {
                    freeSpaces.Add(i);
                }
            }
            dumpSnapshot(room, userData, -1);
            // DEFINEPOSITION SCHEMA.
            if (freeSpaces.Count > 0)
            {
                this.definePosition(userData.sessID, freeSpaces).ConfigureAwait(false);
                this.playerInfo(userData);
                // DEPOSIT SCHEMA -- see documentation of eppr
                //sessionHandler.sendToSessID("GameController/deposit", userData.sessID, roomAuthProtocol.getDepositSchema());
                //Clients.Clients(userData.sessID).SendAsync("GameController/deposit", new RequestDeposit());
            }
            else
            {
                foreach (UserData spectator in room.Users)
                {
                    if (spectator.dataBlock.PlayerId == userData.dataBlock.PlayerId)
                    {
                        room.Users.Remove(spectator);
                    }
                }
                room.Users.Add(userData);
                //sessionHandler.sendToSessID("GameController/rejectFullyfied", userData.sessID, this.gameProtocol.getRejectFullyfiedSchema());
                Clients.Clients(userData.sessID).SendAsync("GameController/rejectFullyfied", this.gameProtocol.getRejectFullyfiedSchema());
            }
        }

        public void sitFlow(int position, UserData userData, RoomModel room)
        {
            if (position >= room.maxPlayers || position < 0 || room.PlayerArray[position] != null)
            {
                List<int> freeSpaces = new List<int>();
                for (int i = 0; i < room.maxPlayers; i++)
                {
                    if (room.PlayerArray[i] == null)
                    {
                        freeSpaces.Add(i);
                    }
                }
                if (freeSpaces.Count > 0)
                {
                    Clients.Clients(userData.sessID).SendAsync("GameController/rejectedPosition", gameProtocol.getRejectedPositionSchema(freeSpaces));
                }
                else
                {
                    Clients.Clients(userData.sessID).SendAsync("GameController/rejectFullyfied", gameProtocol.getRejectFullyfiedSchema());
                }
            }
            else
            {
                if (!Utils.isPlayerExistsInTablePosition(position, room, userData))
                {
                    room.PlayerArray[position] = userData;

                    var userInRoom = new UserInRoom()
                    {
                        UserId = userData.userID,
                        RoomId = room.roomId,
                        MoneyInRoom = Convert.ToDecimal(userData.chips),
                        Position = position,
                        Registered = new DateTime()
                    };
                    
                    var room1 = _gameLogSvc.AddUserInRoom(userInRoom, room.roomId);
                    _loobyHubcontext.Clients.Group("Lobby").SendAsync("LobbyListener", room1);
                    ingressSchema(position, userData, room);
                }
                else
                {
                    Clients.Clients(userData.sessID).SendAsync("GameController/rejectFullyfied", gameProtocol.getRejectFullyfiedSchema());
                }
            }
        }

        public void ingressSchema(int position, UserData userData, RoomModel room)
        {
            // INGRESS SCHEMA
            if (userData.chips == 0)
            {
                //sessionHandler.sendToSessID("GameController/deposit", userData.sessID, roomAuthProtocol.getDepositSchema());
                Clients.Clients(userData.sessID).SendAsync("GameController/deposit", new RequestDeposit());
                return;
            }
            var gameTest = gameProtocol.getIngressSchema(userData, position);
            var getAnnouncment = gameProtocol.getAnnouncementSchema(userData, position);
            Clients.Clients(userData.sessID).SendAsync("GameController/ingress", gameProtocol.getIngressSchema(userData, position));


            // Announcement
            Clients.Group(room.roomId.ToString()).SendAsync("GameController/announcement", gameProtocol.getAnnouncementSchema(userData, position));


            CheckStartGame(room);
        }

        private async Task definePosition(List<string> sessID, List<int> freeSpaces)
        {
            await _roomHubContext.Clients.Clients(sessID).SendAsync("GameController/definePosition", this.gameProtocol.getDefinePositionSchema(freeSpaces));
        }

        private void playerInfo(UserData userData)
        {
            var data = new { chips = userData.dataBlock.Chips };
            _roomHubContext.Clients.Clients(userData.sessID).SendAsync("GameController/playerChips", data);
        }

        private void definePosition(List<string> sessID, RoomModel room)
        {
            List<int> freeSpaces = new List<int>();
            for (int i = 0; i < room.maxPlayers; i++)
                if (room.PlayerArray[i] == null)
                    freeSpaces.Add(i);

            if (freeSpaces.Count > 0)
                definePosition(sessID, freeSpaces);
            else
                _roomHubContext.Clients.Clients(sessID).SendAsync("GameController/rejectFullyfied", this.gameProtocol.getRejectFullyfiedSchema());
        }

        public async Task AddChipsToUser(string userID, double chips, long accountChips, RoomModel room, int position)
        {
            bool done = false;
            UserData uD = null;
            for (int i = 0; i < room.maxPlayers; i++)
            {
                if (room.PlayerArray[i] != null && room.PlayerArray[i].dataBlock.PlayerId == int.Parse(userID))
                {
                    //room.PlayerArray[i].chips += chips;
                    uD.chips = Convert.ToInt64(await _gameLogSvc.UpdateUserChips(accountChips, int.Parse(userID), false));

                    await onDeposit(room.PlayerArray[i], accountChips, room);
                    CheckStartGame(room);


                    uD = room.PlayerArray[i];
                    done = true;
                }
            }
            if (!done)
            {

                //not in table.
                uD = room.Users.Where(_ => _.dataBlock.PlayerId == int.Parse(userID)).FirstOrDefault();
                if (uD != null)
                {

                    //uD.dataBlock.Chips -= chips;
                    var playerChips = Convert.ToInt64(await _gameLogSvc.UpdateUserChips(chips, int.Parse(userID), true));
                    uD.chips = chips;
                    //onDeposit(uD, chips, room);

                    var data = new { chips = playerChips };
                    await _roomHubContext.Clients.Clients(uD.sessID).SendAsync("RefreshChips", data);

                    sitFlow(position, uD, room);
                }
                else
                {
                    // TODO: refound chips.

                    //logger.error("Not user in memory for deposit: UID: " + userID + " Chips: " + chips + " AccountChips: " + accountChips);
                }
            }
        }


        #endregion

        #region PokerRoom
        static int Timer = 10;
        public void CheckStartGame(RoomModel room)
        {
            //log.debug("Check Start Game");
            if (!room.inGame && Utils.checkPlayers(room.PlayerArray) >= 2)//&& room.Users.Where(_ => _.challengeAction == ChallengeActions.DEPOSIT).ToList().Count > 1
            { // FIXME: change this, get from configuration, is in two only for test/dev purposes.
              // START GAME
                processLeaveRequests(room, false);
                //log.debug("START GAME");
                StartGame startGame = new StartGame
                {
                    // FIXME: put this value in the configuration file.
                    startIn = 10
                };

                _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/startGame", startGame);


                room.roundModel = null;

                System.Timers.Timer timer = new System.Timers.Timer
                {
                    Interval = 10000,
                    AutoReset = false
                };
                timer.Start();

                timer.Elapsed += ((o, e) =>
                {
                    StartRound(room);
                });
            }
        }

        private void StartRoundWithTimer(RoomModel room)
        {
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 5000,
                AutoReset = false
            };
            timer.Start();
            room.inGame = false;
            room.roundModel = null;

            timer.Elapsed += ((o, e) =>
            {
                StartRound(room);
            });
        }

        private void StartRound(RoomModel room)
        {
            // TODO: ignore players sitted but without deposit in usersInGame:
            processLeaveRequests(room, false);
            // define the dealer

            try
            {

                if (Utils.countUsersCanPlay(room.PlayerArray) > 1)
                {
                    if (room.inGame == false)
                    {
                        room.inGame = true;
                        var roomDetailsDb = _gameLogSvc.GetRoomRoomDetails(room.roomId);
                        room.BIG_BLIND = roomDetailsDb.BigBlind;
                        room.SMALL_BLIND = roomDetailsDb.SmallBlind;
                        room.MAX_RAISE = roomDetailsDb.MaxRaise;
                        room.MIN_RAISE = roomDetailsDb.MinRaise;
                        room.BIG_BLIND = roomDetailsDb.BigBlind;
                       

                        room.roundModel = new RoundModel
                        {
                            dealerPosition = Utils.getRandomPositionOfTakens(room.PlayerArray),
                            FeeForRound = roomDetailsDb.FeePercentage,
                        };

                        room.roundModel.dealerPosition = Utils.getNextPositionOfPlayers(room.PlayerArray, room.roundModel.dealerPosition);
                        //room.roundModel.rounds.a(actualRound);
                        InitRoundGame(new Deck(), room.PlayerArray, room.roundModel.dealerPosition, room);//.Where(_ => _.challengeAction == ChallengeActions.DEPOSIT).ToList()
                        if (Start(room))
                        {
                            room.inGame = false;
                            StartRound(room);
                        }

                    }

                }
                else
                {
                    room.inGame = false;

                    StartGame startGame = new StartGame
                    {
                        startIn = -1 // initial time
                    };
                    //room.roundModel.actualRound = null;
                    //sessionHandler.sendToAll("/GameController/startGame", startGame);
                    _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/startGame", startGame);
                    Utils.getPlayersWithoutChips(room.PlayerArray).ForEach(user =>
                    {
                        //sessionHandler.sendToSessID("GameController/deposit", user.sessID, new RequestDeposit());
                        room.leaveRequests.Add(user);
                        processLeaveRequests(room, true);
                        //await _roomHubContext._roomHubContext._roomHubContext.Clients._roomHubContext._roomHubContext.Clients(user.sessID).SendAsync("GameController/deposit",user.sessID, new RequestDeposit());
                    });
                }

            }
            catch (Exception ex)
            {
                _gameLogSvc.CreateLogMessage("RoomHub", "StartRound", ex);

            }
        }

        private async Task processLeaveRequests(RoomModel room, bool isWithoutChips)
        {
            foreach (var item in room.leaveRequests)
            {
                await leavePlayerIf(item, room, isWithoutChips);
            }
            room.leaveRequests.Clear();
        }

        private async Task leavePlayerIf(UserData ud, RoomModel room, bool isWithoutChips)
        {
            int ppos = Utils.getPlyerPosition(room.PlayerArray, ud);
            // enviar fichas al orchestrator.
            if (ppos != -1)
            {
                if (room.PlayerArray[ppos] != null)
                {
                    await RefoundChips(room, room.PlayerArray[ppos]);
                }

                var ln = new LeaveNotify
                {
                    position = ppos
                };
                List<int> freeSpaces = new List<int>();

                for (int i = 0; i < room.maxPlayers; i++)
                    if (room.PlayerArray[i] == null)
                        freeSpaces.Add(i);

                await this.definePosition(room.PlayerArray[ppos].sessID, freeSpaces);

                var userInRoom = new UserInRoom()
                {
                    UserId = room.PlayerArray[ppos].userID,
                    RoomId = room.roomId,
                    MoneyInRoom = Convert.ToDecimal(room.PlayerArray[ppos].chips),
                    Position = ppos,
                    Registered = new DateTime()
                };

                var room1 = await _gameLogSvc.RemoveUserInRoom(userInRoom, room.roomId);
                await _loobyHubcontext.Clients.Group("Lobby").SendAsync("LobbyListener", room1);

                if (isWithoutChips)
                {
                    await _roomHubContext.Clients.Clients(room.PlayerArray[ppos].sessID).SendAsync("GameController/deposit", new RequestDeposit() { position = ppos, id = room.PlayerArray[ppos].dataBlock.PlayerId });
                }

                room.PlayerArray[ppos] = null;

                await _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/leaveNotify", ln);
       
            }
        }

        private async Task RefoundChips(RoomModel room, UserData user)
        {
            var chips = await _gameLogSvc.UpdateUserChips(user.chips, user.dataBlock.PlayerId, false);

            var data = new { chips = chips };
            await _roomHubContext.Clients.Clients(user.sessID).SendAsync("RefreshChips", data);
        }

        public async Task onDeposit(UserData player, long chipsDeposited, RoomModel room)
        {
            // TODO Auto-generated method stub

            for (int i = 0; i < room.PlayerArray.Length; i++)
                if (room.PlayerArray[i] != null && room.PlayerArray[i].dataBlock.PlayerId == player.dataBlock.PlayerId)
                {
                    DepositAnnouncement da = new DepositAnnouncement(i, chipsDeposited);
                    await _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/depositAnnouncement", da);
                }
        }

        public void dumpSnapshot(RoomModel room, UserData user, int i)
        {
            // TODO - CHAMGE ROUND
            _roomHubContext.Clients.Clients(user.sessID).SendAsync("/GameController/snapshot", getSnapshot(room.roomId.ToString(), user.dataBlock.PlayerId.ToString(), i));
        }

        private Snapshot getSnapshot(string roomId, string userId, int pos)
        {
            Rooms.TryGetValue(roomId, out var room);
            Snapshot snap = new Snapshot
            {
                players = new List<SnapshotPlayer>(),
                myPosition = pos,
                bigBlind = room.BIG_BLIND, 
                smallBlind = room.SMALL_BLIND
            };
            for (int i = 0; i < room.PlayerArray.Length; i++)
            {
                if (room.PlayerArray[i] != null)
                {
                    SnapshotPlayer player = new SnapshotPlayer
                    {
                        chips = room.PlayerArray[i].chips,
                        nick = room.PlayerArray[i].dataBlock.UserName,
                        photo = "https://images.pexels.com/photos/3200602/pexels-photo-3200602.jpeg?auto=compress&cs=tinysrgb&dpr=2&h=650&w=940",
                        inGame = false,
                        userID = room.PlayerArray[i].dataBlock.PlayerId
                    };
                    if (room.roundModel != null)
                    {
                        player.actualBet = getBetOf(i, room);
                        player.showingCards = false;
                        player.haveCards = haveCards(i, room);
                        player.inGame = isInGame(i, room);
                        // Me?
                        if (i == snap.myPosition || snap.myPosition < 0)
                        {
                            player.showingCards = true;
                            player.cards = getCards(i, room);
                        }
                        //player.showingCards?
                        //player.cards = getCardsForUser with player.showingCards?
                    }
                    snap.players.Add(player);
                }
                else
                    snap.players.Add(null);
            }
            snap.isDealing = false;
            snap.isInRest = false;
            if (room.roundModel != null && room.inGame)
            {
                snap.isInRest = true;
                snap.dealerPosition = getDealerPosition(room);
                List<double> pots = Utils.getPotValues(getPot(room));
                snap.pots = pots; // get actual pot
                Card[] cards = getCommunityCards(room);
                snap.communityCards = new List<SchemaCard>();

                for (int i = 0; i < cards.Length; i++)
                    snap.communityCards.Add(Utils.getSchemaFromCard(cards[i]));

                snap.roundStep = getStep(room);
                // action?
                if (checkWaiting(room))
                {
                    snap.waitingFor = getWaitingActionFromPlayer(room);
                    snap.waitingForTimer = getWaitingTimerFromPlayer(room);
                    Console.WriteLine("SNAP Waiting For Player Seconds: " + snap.waitingForTimer.ToString());
                    // is me?
                    if (snap.myPosition == snap.waitingFor)
                        snap.betDecision = calcDecision(room);
                }
            }
            return snap;
        }
        #endregion

        #region RoundGame
        public void InitRoundGame(Deck deck, UserData[] usersInGame, int dealerPosition, RoomModel room)
        {
            room.roundModel.usersInGame = new UserData[room.maxPlayers];
            usersInGame.CopyTo(room.roundModel.usersInGame, 0);

            room.roundModel.usersInGameDescriptor = new UserMetaData[room.roundModel.usersInGame.Length];

            for (int i = 0; i < room.roundModel.usersInGame.Length; i++)
                if (room.roundModel.usersInGame[i] != null)
                    room.roundModel.usersInGameDescriptor[i] = new UserMetaData();

            //this.bets = ArrayUtils.toPrimitive(Collections.nCopies(usersInGame.Length, 0L).toArray(new long[0]));
            //this.bets = convertIntegers(Collections.nCopies(usersInGame.Length, 0L).toArray(new long[0]));
            room.roundModel.bets = new double[room.roundModel.usersInGame.Length];

            room.roundModel.playerFirstCards = new Card[room.roundModel.usersInGame.Length];
            room.roundModel.playerSecondCards = new Card[room.roundModel.usersInGame.Length];
            room.roundModel.deck = deck;
            deck.shuffle();
            room.roundModel.dealerPosition = dealerPosition;
            room.roundModel.rounds++;
        }

        public static int[] convertIntegers(List<int> integers)
        {
            int[] ret = new int[integers.Count];

            for (int i = 0; i < ret.Length; i++)
                ret[i] = integers[i];

            return ret;
        }

        public bool Start(RoomModel room)
        {

            RoundStart roundStartSchema = new RoundStart
            {
                dealerPosition = room.roundModel.dealerPosition,
                roundNumber = room.roundModel.rounds
            };
            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/roundStart", roundStartSchema);
            //sessionHandler.sendToAll("/GameController/roundStart", roundStartSchema);

            bool AllInCornerCase = requestBlind(room.SMALL_BLIND, room.BIG_BLIND, room); // FXIME: adjust according to configuration.
            try
            {
                dealCards(room);
                room.roundModel.roundStep = 1; // pre-flop
                if (!AllInCornerCase)
                {
                    sendWaitAction(room);
                    return false;
                }
                else
                {
                    showOff(room);
                    threadWait(500);
                    return finishBets(room);
                }
            }
            catch (System.NullReferenceException e)
            {
                Console.WriteLine("Interrupted Exception ", e);
                // FIXME: if this explode, then the cards are never ends to dealing.
            }
            return false;
        }

        private bool requestBlind(int smallBlindSize, int bigBlindSize, RoomModel room)
        {

            int smallBlind = Utils.getNextPositionOfPlayers(room.roundModel.usersInGame, room.roundModel.dealerPosition);
            room.roundModel.bigBlind = Utils.getNextPositionOfPlayers(room.roundModel.usersInGame, smallBlind);
            room.roundModel.lastRise = bigBlindSize;
            PokerLogic.Models.SchemaModels.inGame.Blind blindObject = new PokerLogic.Models.SchemaModels.inGame.Blind
            {
                sbPosition = smallBlind,
                sbChips = smallBlindSize,
                bbPosition = room.roundModel.bigBlind,
                bbChips = bigBlindSize
            };

            if (room.roundModel.usersInGame[smallBlind].chips > smallBlindSize)
            {
                room.roundModel.usersInGame[smallBlind].chips -= smallBlindSize;
                room.roundModel.bets[smallBlind] = smallBlindSize;
            }
            else
            {
                // ALL IN
                room.roundModel.bets[smallBlind] = room.roundModel.usersInGame[smallBlind].chips;
                blindObject.sbChips = room.roundModel.bets[smallBlind];
                room.roundModel.usersInGameDescriptor[smallBlind].isAllIn = true;
                room.roundModel.usersInGame[smallBlind].chips = 0;

            }

            if (room.roundModel.usersInGame[room.roundModel.bigBlind].chips > bigBlindSize)
            {
                room.roundModel.usersInGame[room.roundModel.bigBlind].chips -= bigBlindSize;
                room.roundModel.bets[room.roundModel.bigBlind] = bigBlindSize;
            }
            else
            {
                // ALL IN
                room.roundModel.bets[room.roundModel.bigBlind] = room.roundModel.usersInGame[room.roundModel.bigBlind].chips;
                blindObject.bbChips = room.roundModel.bets[room.roundModel.bigBlind];
                room.roundModel.usersInGameDescriptor[room.roundModel.bigBlind].isAllIn = true;
                room.roundModel.usersInGame[room.roundModel.bigBlind].chips = 0;

            }
            //roomHub.Clients.Group("").SendAsync("/GameController/blind", blindObject);

            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/blind", blindObject);


            int firstWAFP = -1;
            room.roundModel.waitingActionFromPlayer = Utils.getNextPositionOfPlayers(room.roundModel.usersInGame, room.roundModel.bigBlind);
            while (room.roundModel.usersInGameDescriptor[room.roundModel.waitingActionFromPlayer].isAllIn && room.roundModel.waitingActionFromPlayer != firstWAFP)
            {
                if (firstWAFP == -1)
                {
                    firstWAFP = room.roundModel.waitingActionFromPlayer;
                }
                room.roundModel.waitingActionFromPlayer = Utils.getNextPositionOfPlayers(room.roundModel.usersInGame, room.roundModel.bigBlind);
            }
            if (room.roundModel.usersInGameDescriptor[room.roundModel.waitingActionFromPlayer].isAllIn)
            {
                // todos en allIn?
                return true;
            }
            if (((room.roundModel.waitingActionFromPlayer == room.roundModel.bigBlind) || (
                 room.roundModel.waitingActionFromPlayer == smallBlind &&
                blindObject.bbChips == smallBlindSize)) &&
                    isAllinAllIn(room))
            {
                // automaticamente cerrar el juego como si todos fueran all in
                return true;
            }

            // en caso de la ciega estar en All-In
            if (room.roundModel.usersInGameDescriptor[room.roundModel.bigBlind].isAllIn)
            {
                room.roundModel.lastActionedPosition = room.roundModel.waitingActionFromPlayer;
                //			ignoreLastActionedPositionOnce = true;
            }
            else
            {
                room.roundModel.lastActionedPosition = room.roundModel.bigBlind;
            }
            return false;
        }

        private void sendWaitAction(RoomModel room)
        {

            room.roundModel.isWaiting = true;
            ActionFor aFor = new ActionFor
            {
                position = room.roundModel.waitingActionFromPlayer,
                remainingTime = 20
            };

            Console.WriteLine("Timer START ");
            if (room.inGame)
            {
                room.roundModel.TimerOfWaitingPlayer = new System.Timers.Timer
                {
                    Interval = 1000,
                    AutoReset = true,
                    Enabled = true
                };
                room.roundModel.waitingTimerFromPlayer = 20;
                room.roundModel.TimerOfWaitingPlayer.Elapsed += ((o, e) =>
                {
                    if (room.roundModel != null)
                    {
                        room.roundModel.waitingTimerFromPlayer = room.roundModel.waitingTimerFromPlayer - 1;
                        Console.WriteLine("Waiting For Player: " + room.roundModel.waitingActionFromPlayer + " seconds: " + room.roundModel.waitingTimerFromPlayer);

                        if (room.roundModel.waitingTimerFromPlayer <= 0)
                        {
                            room.roundModel.isFinishedPlayerDidntPlay = true;
                            room.roundModel.waitingTimerFromPlayer = 20;
                            bool finishedRound = processDecision(new DecisionInform() { action = "fold", ammount = 0, position = room.roundModel.waitingActionFromPlayer }, room.PlayerArray[room.roundModel.waitingActionFromPlayer].dataBlock.PlayerId.ToString(), room);

                            var player = room.PlayerArray[room.roundModel.waitingActionFromPlayer];
                            room.leaveRequests.Add(player);

                            processLeaveRequests(room, false).ConfigureAwait(true);
                            if (finishedRound)
                            {
                                StartRoundWithTimer(room);
                            }


                        }
                    }
                });


                _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/actionFor", aFor);
                // sessionHandler.sendToAll("/GameController/actionFor", aFor);

                _roomHubContext.Clients.Clients(room.roundModel.usersInGame[aFor.position].sessID).SendAsync("GameController/betDecision", calcDecision(room));
                // sessionHandler.sendToSessID("GameController/betDecision", usersInGame[aFor.position].sessID, calcDecision());
            }

        }

        public BetDecision calcDecision(RoomModel room)
        {
            // action for:
            BetDecision bd = new BetDecision
            {
                toCall = room.roundModel.lastRise - room.roundModel.bets[room.roundModel.waitingActionFromPlayer]
            };
            bd.canCheck = bd.toCall == 0;
            bd.minRaise = room.MIN_RAISE;
            bd.maxRaise = room.MAX_RAISE;
            return bd;
        }

        private void dealCards(RoomModel room)
        {
            IList<int> players = Utils.getPlayersFromPosition(room.roundModel.usersInGame, room.roundModel.dealerPosition);
            //deck.getNextCard(); // burn a card ?.
            // first iteration:
            int lastPosition = 1;
            try
            {
                foreach (int position in players)
                {
                    room.roundModel.playerFirstCards[position] = room.roundModel.deck.getNextCard();
                    CardDist cd = new CardDist
                    {
                        position = position,
                        cards = new bool[] { true, false }
                    };
                    // sessionHandler.sendToAll("/GameController/cardsDist", cd); // to all
                    _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/cardsDist", cd);
                    ICardDist icd = new ICardDist
                    {
                        position = position
                    };
                    lastPosition = position;
                    SchemaCard stCard = Utils.getSchemaFromCard(room.roundModel.playerFirstCards[position]);
                    icd.cards = new SchemaCard[] { stCard, null };
                    _roomHubContext.Clients.Clients(room.roundModel.usersInGame[position].sessID).SendAsync("GameController/cardsDist", icd);
                    //sessionHandler.sendToSessID("GameController/cardsDist", usersInGame[position].sessID, icd); // to the player
                    // wait a moment?
                }
                // second iteration:
                foreach (int position in players)
                {
                    room.roundModel.playerSecondCards[position] = room.roundModel.deck.getNextCard();
                    CardDist cd = new CardDist
                    {
                        position = position,
                        cards = new bool[] { true, true }
                    };
                    // sessionHandler.sendToAll("/GameController/cardsDist", cd); // to all
                    _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/cardsDist", cd);
                    ICardDist icd = new ICardDist
                    {
                        position = position
                    };
                    lastPosition = position;
                    SchemaCard stCard = Utils.getSchemaFromCard(room.roundModel.playerFirstCards[position]);
                    SchemaCard ndCard = Utils.getSchemaFromCard(room.roundModel.playerSecondCards[position]);
                    icd.cards = new SchemaCard[] { stCard, ndCard };
                    // sessionHandler.sendToSessID("GameController/cardsDist", usersInGame[position].sessID, icd); // to the player
                    _roomHubContext.Clients.Clients(room.roundModel.usersInGame[position].sessID).SendAsync("GameController/cardsDist", icd);
                    // wait a moment?
                }
            }
            catch (System.NullReferenceException npe)
            {
                Console.WriteLine("npe: " + lastPosition, npe);
            }
        }

        // Return if the round is finished6
        public bool processDecision(DecisionInform dI, string userId, RoomModel room)
        {
            try
            {

                var uD = Utils.getPlayerById(room, int.Parse(userId));

                dI.position = Utils.getPlyerPosition(room.roundModel.usersInGame, uD);
                //Timer
                Console.WriteLine("Timer STOP");
                room.roundModel.TimerOfWaitingPlayer.Stop();
                room.roundModel.TimerOfWaitingPlayer.Dispose();
                room.roundModel.TimerOfWaitingPlayer.Enabled = false;
                room.roundModel.waitingTimerFromPlayer = 20;

                room.roundModel.isWaiting = false;
                if (dI.position == room.roundModel.waitingActionFromPlayer)
                {
                    bool actionDoed = false;
                    bool finishedBets = false;
                    // check if zero:
                    if ("raise".EqualsIgnoreCase(dI.action) && dI.ammount <= 0)
                        dI.action = "call";
                    if ("fold".EqualsIgnoreCase(dI.action))
                    {
                        // TODO: remove me from all pots winners.
                        room.roundModel.pots.ForEach(pot =>
                        {
                            for (int i = 0; i < pot.playersForPot.Count; i++)
                                if (pot.playersForPot[i] == dI.position)
                                    pot.playersForPot.Remove(i);
                        });

                        room.roundModel.usersInGame[dI.position] = null; // fold user.
                        FoldDecision fd = new FoldDecision
                        {
                            position = dI.position
                        };
                        _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/fold", fd);
                        if (checkPlayerActives(room) > 1)
                            actionDoed = true;
                        else
                            return finishBetFullFold(room);
                    }
                    if ("call".EqualsIgnoreCase(dI.action))
                    {
                        double realBet = room.roundModel.lastRise - room.roundModel.bets[dI.position];
                        if (room.roundModel.usersInGame[dI.position].chips >= realBet)
                        {
                            room.roundModel.usersInGame[dI.position].chips -= realBet;
                            if (room.roundModel.usersInGame[dI.position].chips == 0)
                                room.roundModel.usersInGameDescriptor[dI.position].isAllIn = true;

                            actionDoed = true;
                            dI.ammount = realBet; // change the ammount to real count for frontend
                            room.roundModel.bets[dI.position] = room.roundModel.lastRise;
                        }
                        else
                        {
                            // TODO: review this.
                            dI.ammount = room.roundModel.usersInGame[dI.position].chips;
                            room.roundModel.usersInGameDescriptor[dI.position].isAllIn = true;
                            actionDoed = true;
                            room.roundModel.bets[dI.position] += room.roundModel.usersInGame[dI.position].chips;
                            room.roundModel.usersInGame[dI.position].chips = 0;
                        }
                    }
                    if ("check".EqualsIgnoreCase(dI.action))
                    {
                        if (room.roundModel.lastRise == room.roundModel.bets[dI.position])
                        {
                            actionDoed = true;
                            //lastActionedPosition = dI.position;
                        }
                        else
                        {
                            room.roundModel.pots.ForEach(pot =>
                            {
                                for (int i = 0; i < pot.playersForPot.Count; i++)
                                    if (pot.playersForPot[i] == dI.position)
                                        pot.playersForPot.Remove(i);
                            });

                            room.roundModel.usersInGame[dI.position] = null; // fold user.
                            FoldDecision fd = new FoldDecision
                            {
                                position = dI.position
                            };

                            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/fold", fd);
                            if (checkPlayerActives(room) > 1)
                                actionDoed = true;
                            else
                                return finishBetFullFold(room);
                        }
                    }
                    if ("raise".EqualsIgnoreCase(dI.action))
                    {
                        // TODO: check maximums and minimums.
                        double ammount = dI.ammount;
                        double initialBet = room.roundModel.lastRise - room.roundModel.bets[dI.position];
                        room.roundModel.lastActionedPosition = dI.position;
                        double totalAmmount = initialBet + ammount;
                        if (room.roundModel.usersInGame[dI.position].chips >= totalAmmount)
                        {
                            room.roundModel.usersInGame[dI.position].chips -= totalAmmount;

                            if (room.roundModel.usersInGame[dI.position].chips == 0)
                                room.roundModel.usersInGameDescriptor[dI.position].isAllIn = true;

                            dI.ammount = totalAmmount; // change the ammount to real count for frontend
                            actionDoed = true;
                            room.roundModel.bets[dI.position] += totalAmmount;
                            room.roundModel.lastRise = room.roundModel.bets[dI.position];
                            room.roundModel.bigBlind = -1;
                        }
                        else
                        {
                            // TODO: Review this.
                            room.roundModel.bets[dI.position] += room.roundModel.usersInGame[dI.position].chips;
                            dI.ammount = initialBet + room.roundModel.usersInGame[dI.position].chips;
                            room.roundModel.usersInGame[dI.position].chips = 0;
                            room.roundModel.usersInGameDescriptor[dI.position].isAllIn = true;
                            actionDoed = true;
                            room.roundModel.lastRise = room.roundModel.bets[dI.position];
                            room.roundModel.bigBlind = -1;
                        }
                    }

                    if (actionDoed)
                    {
                        int nextPosition = Utils.getNextPositionOfPlayers(room.roundModel.usersInGame, dI.position);
                        if ("raise".EqualsIgnoreCase(dI.action))
                        {
                            finishedBets = nextPlayer(nextPosition, room);
                        }
                        else
                        {
                            if (isAllinAllIn(room))
                            {
                                finishedBets = true;
                                showOff(room);
                                threadWait(500); // TODO: parametize this
                            }
                            else
                            {
                                if (nextPosition == room.roundModel.bigBlind)
                                {
                                    finishedBets = nextPlayer(nextPosition, room);
                                }
                                else if (room.roundModel.lastActionedPosition == nextPosition || dI.position == room.roundModel.bigBlind)
                                { // if next is last or actual is last (in bigBlind case)
                                    finishedBets = true;
                                }
                                else
                                {
                                    finishedBets = nextPlayer(nextPosition, room);
                                }
                            }

                        }

                        _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/decisionInform", dI);
                        if (finishedBets)
                        {
                            return finishBets(room);
                        }
                    }
                    else
                    {
                        // TODO: error message?
                    }
                }
                else if (dI.isLeaving)
                {
                    bool finishedBets = false;
                    bool actionDoed = false;
                    if ("fold".EqualsIgnoreCase(dI.action))
                    {

                        // TODO: remove me from all pots winners.
                        room.roundModel.pots.ForEach(pot =>
                        {
                            for (int i = 0; i < pot.playersForPot.Count; i++)
                                if (pot.playersForPot[i] == dI.position)
                                    pot.playersForPot.Remove(i);
                        });

                        room.roundModel.usersInGame[dI.position] = null; // fold user.
                        FoldDecision fd = new FoldDecision
                        {
                            position = dI.position
                        };
                        _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/fold", fd);
                        if (checkPlayerActives(room) > 1)
                            actionDoed = true;
                        else
                            return finishBetFullFold(room);
                    }

                    int nextPosition = Utils.getNextPositionOfPlayers(room.roundModel.usersInGame, dI.position);
                    if ("raise".EqualsIgnoreCase(dI.action))
                    {
                        finishedBets = nextPlayer(nextPosition, room);
                    }
                    else
                    {
                        if (isAllinAllIn(room))
                        {
                            finishedBets = true;
                            showOff(room);
                            threadWait(500); // TODO: parametize this
                        }
                        else
                        {
                            if (nextPosition == room.roundModel.bigBlind)
                            {
                                finishedBets = nextPlayer(nextPosition, room);
                            }
                            else if (room.roundModel.lastActionedPosition == nextPosition || dI.position == room.roundModel.bigBlind)
                            { // if next is last or actual is last (in bigBlind case)
                                finishedBets = true;
                            }
                            else
                            {
                                finishedBets = nextPlayer(nextPosition, room);
                            }
                        }

                    }

                    _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/decisionInform", dI);
                    if (finishedBets)
                    {
                        return finishBets(room);
                    }
                }

            }
            catch (Exception ex)
            {
                _gameLogSvc.CreateLogMessage("RoomHub", "StartRound", ex);

            }

            return false;
        }

        private int checkPlayerActives(RoomModel room)
        {
            int count = 0;
            for (int i = 0; i < room.roundModel.usersInGame.Length; i++)
            {
                if (room.roundModel.usersInGame[i] != null)
                {
                    count++;
                    room.roundModel.lastActivePositionDetected = i;
                }
            }
            return count;
        }

        private bool finishBetFullFold(RoomModel room)
        {
            // check round bets:
            double pot = 0;
            for (int i = 0; i < room.roundModel.bets.Length; i++)
                pot += room.roundModel.bets[i];

            foreach (Pot potObj in room.roundModel.pots)
                pot += potObj.pot;

            ResultSet rs = new ResultSet
            {
                winners = new List<Winner>()
            };

            int winner = room.roundModel.lastActivePositionDetected;
            Winner winnerData = new Winner
            {
                points = 0,
                position = winner,
                pot = pot,
                reason = "All other players fold"
            };


            room.roundModel.usersInGame[winner].chips += pot;
            rs.winners.Add(winnerData);
            var newUserArray = new UserData[room.maxPlayers];
            room.roundModel.usersInGame.CopyTo(newUserArray, 0);

            AddRoundToDB(room, rs, newUserArray);

            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/resultSet", rs);
            threadWait(1000);

            //if(room.roundModel != null)
            //    if (room.roundModel.isFinishedPlayerDidntPlay)
            //    {
            //        room.roundModel.isFinishedPlayerDidntPlay = false;
            //        StartRoundWithTimer(room);
            //    }


            return true;
        }

        private bool finishBets(RoomModel room)
        {
            room.roundModel.bigBlind = -1;
            int nextPj = Utils.getNextPositionOfPlayers(room.roundModel.usersInGame, room.roundModel.dealerPosition);
            room.roundModel.lastActionedPosition = nextPj;
            // resetting rises:
            room.roundModel.lastRise = 0;
            // merge de pots:
            List<Pot> newPots = SplitAndNormalizedPots(room);
            if (newPots.Count > 0)
            {
                if (room.roundModel.pots.Count > 0)
                {
                    if (room.roundModel.pots[room.roundModel.pots.Count - 1].playersForPot.Count == newPots[0].playersForPot.Count)
                    {
                        // si son los mismos jugadores unimos los pozos.
                        room.roundModel.pots[room.roundModel.pots.Count - 1].pot += newPots[0].pot;
                        newPots.RemoveAt(0);
                    }
                    room.roundModel.pots.AddRange(newPots);
                }
                else
                {
                    room.roundModel.pots.AddRange(newPots);
                }
            }
            // mandamos al front la lista de pots:
            Pots schemaPots = new Pots
            {
                pots = Utils.getPotValues(room.roundModel.pots)
            };
            // mandamos al front el nuevo estado de fichas
            updateChips(room);
            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/pots", schemaPots);
            if (room.roundModel.roundStep == 1)
            {
                // flop:
                room.roundModel.roundStep = 2;
                // wait a moment?
                threadWait(500); // TODO: parameterize
                dealFlop(room);
                if (isAllinAllIn(room))
                {
                    threadWait(500); // TODO: parameterize
                    return finishBets(room);
                }
                else
                {
                    nextPlayer(nextPj, room);
                }
            }
            else if (room.roundModel.roundStep == 2)
            {
                // turn:
                // wait a moment?
                threadWait(500); // TODO: parameterize
                room.roundModel.roundStep = 3;
                dealTurn(room);
                if (isAllinAllIn(room))
                {
                    threadWait(500); // TODO: parameterize
                    return finishBets(room);
                }
                else
                {
                    nextPlayer(nextPj, room);
                }
            }
            else if (room.roundModel.roundStep == 3)
            {
                // turn:
                // wait a moment?
                threadWait(500); // TODO: parameterize
                room.roundModel.roundStep = 4;
                dealRiver(room);
                if (isAllinAllIn(room))
                {
                    threadWait(500); // TODO: parameterize
                    return finishBets(room);
                }
                else
                {
                    nextPlayer(nextPj, room);
                }
            }
            else if (room.roundModel.roundStep == 4)
            {
                Console.WriteLine("-- SHOWDOWN --");
                showOff(room);

                checkHands(room.roundModel.pots, room);

                updateChips(room);

                // TODO: parametize this.
                return true;
            }
            return false;
        }

        private void showOff(RoomModel room)
        {
            ShowOff soff = new ShowOff(room.roundModel.usersInGame.Length);
            for (int i = 0; i < room.roundModel.usersInGame.Length; i++)
                if (room.roundModel.usersInGame[i] != null)
                    soff.setCards(
                            i,
                            Utils.getSchemaFromCard(room.roundModel.playerFirstCards[i]),
                            Utils.getSchemaFromCard(room.roundModel.playerSecondCards[i])
                    );

            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/showOff", soff);
        }

        private bool nextPlayer(int nextPosition, RoomModel room)
        {
            if (room.roundModel.usersInGameDescriptor[nextPosition].isAllIn)
            {
                do
                {
                    nextPosition = Utils.getNextPositionOfPlayers(room.roundModel.usersInGame, nextPosition);
                } while (room.roundModel.usersInGameDescriptor[nextPosition].isAllIn && nextPosition != room.roundModel.lastActionedPosition);
                if (nextPosition == room.roundModel.lastActionedPosition)
                {
                    return true; // fin de la mano
                }
                else
                {
                    room.roundModel.waitingActionFromPlayer = nextPosition;
                    sendWaitAction(room);
                }
            }
            else
            {
                room.roundModel.waitingActionFromPlayer = nextPosition;
                sendWaitAction(room);
            }
            return false;
        }

        private void dealFlop(RoomModel room)
        {
            room.roundModel.deck.getNextCard(); // burn a card 
            FlopBegins fb = new FlopBegins();
            SchemaCard[] schemaCards = new SchemaCard[3];
            room.roundModel.flop = new Card[3];
            for (int i = 0; i < 3; i++)
            {
                room.roundModel.flop[i] = room.roundModel.deck.getNextCard();
                schemaCards[i] = Utils.getSchemaFromCard(room.roundModel.flop[i]);
            }
            fb.cards = schemaCards;
            // flop begins:
            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/flop", fb);
        }

        private void dealTurn(RoomModel room)
        {
            room.roundModel.deck.getNextCard(); // burn a card 
            room.roundModel.turn = room.roundModel.deck.getNextCard();
            TurnBegins tb = new TurnBegins
            {
                card = Utils.getSchemaFromCard(room.roundModel.turn)
            };
            // turn begins:
            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/turn", tb);
        }

        private void dealRiver(RoomModel room)
        {
            room.roundModel.deck.getNextCard(); // burn a card 
            room.roundModel.river = room.roundModel.deck.getNextCard();
            RiverBegins rb = new RiverBegins
            {
                card = Utils.getSchemaFromCard(room.roundModel.river)
            };
            // river begins:
            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/river", rb);
        }

        private void checkHands(List<Pot> pots, RoomModel room)
        {
            room.roundModel.hands = new HandValues[room.roundModel.usersInGame.Length];
            List<Card> tableCards = new List<Card>();
            for (int i = 0; i < 3; i++)
                tableCards.Add(room.roundModel.flop[i]);

            tableCards.Add(room.roundModel.turn);
            tableCards.Add(room.roundModel.river);
            for (int i = 0; i < room.roundModel.usersInGame.Length; i++)
                if (room.roundModel.usersInGame[i] != null)
                {
                    List<Card> hand = new List<Card>
                    {
                        room.roundModel.playerFirstCards[i],
                        room.roundModel.playerSecondCards[i]
                    };
                    room.roundModel.hands[i] = GetHandValues(tableCards, hand);
                }
            //		Console.WriteLine("Hands: ", hands);
            List<Winner> winners = new List<Winner>();
            List<Winner> prevWinner = null;
            int iteration = 0;
            foreach (var pot in pots)
            {
                prevWinner = getWinnerOf(pot, iteration, room);
                iteration++;
                winners.AddRange(prevWinner);
            }
            room.roundModel.winnersResultSet = new ResultSet
            {
                winners = winners
               
            };
            var newUserArray = new UserData[room.maxPlayers];
            room.roundModel.usersInGame.CopyTo(newUserArray, 0);
            AddRoundToDB(room, room.roundModel.winnersResultSet, newUserArray);
            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/resultSet", room.roundModel.winnersResultSet);
        }

        public static List<Winner> getWinnerOf(Pot pot, int iteration, RoomModel room)
        {
            List<Winner> winnersPositions = new List<Winner>();
            // prev winner for this pot:
            // TODO: IMPROVE prevenir recalcular revisando si los ganadores previos también son ganadores de este
            //
            uint maxPoints = 0;
            int secondaryMaxPoints = 0;
            HandType handWinner = default;
            List<Winner> cleanWinnersPositions = new List<Winner>();
            // traemos los ganadores por jerarquía
            foreach (int player in pot.playersForPot)
            {
                if (room.roundModel.hands[player] == null)
                {
                    continue;
                }
                if (room.roundModel.hands[player].handPoints > maxPoints)
                {
                    Winner winData = new Winner();
                    winnersPositions = new List<Winner>();
                    winData.fullPot = pot.pot;
                    winData.points = room.roundModel.hands[player].handPoints;
                    winData.position = player;
                    winData.reason = room.roundModel.hands[player].handName;
                    winData.secondaryPoints = room.roundModel.hands[player].secondaryHandPoint;
                    winData.potNumber = iteration;
                    maxPoints = room.roundModel.hands[player].handPoints;
                    secondaryMaxPoints = room.roundModel.hands[player].secondaryHandPoint;
                    handWinner = room.roundModel.hands[player].type;
                    winData.playerWinningCards = room.roundModel.hands[player].playerWinningCards;
                    winnersPositions.Add(winData);
                }
                else if (room.roundModel.hands[player].handPoints == maxPoints)
                {
                    Winner winData = new Winner();
                    winData.fullPot = pot.pot;
                    winData.points = room.roundModel.hands[player].handPoints;
                    winData.secondaryPoints = room.roundModel.hands[player].secondaryHandPoint;
                    winData.position = player;
                    winData.potNumber = iteration;
                    winData.reason = room.roundModel.hands[player].handName;
                    winData.playerWinningCards = room.roundModel.hands[player].playerWinningCards;
                    // juegos con doble handPoint como full o par doble que tienen puntos secundarios:

                    winnersPositions.Add(winData);

                }
            }
            cleanWinnersPositions = winnersPositions;

            int countWinners = cleanWinnersPositions.Count;
            cleanWinnersPositions.ForEach(winner =>
            {
                winner.pot = winner.fullPot / countWinners;
                var mstring = room.roundModel.FeeForRound.ToString();

             
                winner.GameFeeCharge = room.roundModel.FeeForRound * winner.pot;
                winner.pot = winner.pot - winner.GameFeeCharge;
                room.roundModel.usersInGame[winner.position].chips += winner.pot;
            });
            return cleanWinnersPositions;
        }



        public List<Pot> SplitAndNormalizedPots(RoomModel room)
        {
            // separamos los pozos:
            List<Pot> pozos = new List<Pot>();
            List<int> activeUsers = Utils.getPlayersOrderedByBets(room.roundModel.bets);
            if (activeUsers.Count > 1)
            {
                List<int> activeUsersWithoutBigBet = Utils.getPlayersOrderedByBets(room.roundModel.bets);
                activeUsersWithoutBigBet.Remove(activeUsersWithoutBigBet.ElementAt(activeUsersWithoutBigBet.Count - 1));

                for (int i = 0; i <= activeUsersWithoutBigBet.Count - 1; i++)
                {
                    // restamos el bet de esta posicion a las siguientes:
                    var index = activeUsersWithoutBigBet[i];
                    if (room.roundModel.bets[index] <= 0) continue;
                    Pot pozo = new Pot
                    {
                        pot = 0
                    };
                    double bet = room.roundModel.bets[index];

                    for (int z = i; z <= activeUsers.Count - 1; z++)
                    {
                        var zindex = activeUsers[z];
                        room.roundModel.bets[zindex] -= bet;
                        pozo.pot += bet;
                        // si foldeo no lo agregamos como jugador
                        if (room.roundModel.usersInGame[zindex] != null)
                            pozo.playersForPot.Add(zindex);
                    }
                    pozos.Add(pozo);
                    bool morePots = false;
                    for (int z = 0; z <= activeUsersWithoutBigBet.Count - 1; z++)
                    {
                        var zindex = activeUsers[z];
                        if (room.roundModel.bets[zindex] > 0)
                            morePots = true;
                    }
                    if (!morePots)
                        break;
                }
                // devolver excedente del mas grande:
                var maxBexPosition = activeUsers[activeUsers.Count - 1];
                var excedent = room.roundModel.bets[maxBexPosition];
                room.roundModel.usersInGame[maxBexPosition].chips += excedent;
                var data = new { chips = excedent };
                _roomHubContext.Clients.Clients(room.roundModel.usersInGame[maxBexPosition].sessID).SendAsync("RefundChips", data);
            }
            // unimos pozos fantasmass
            List<Pot> ghostPots = new List<Pot>();
            pozos.ForEach(pozo =>
            {
                if (ghostPots.Count > 0)
                {
                    var lastPozo = ghostPots[ghostPots.Count - 1];
                    if (lastPozo.playersForPot.Count == pozo.playersForPot.Count)
                        lastPozo.pot += pozo.pot;
                    else
                        ghostPots.Add(pozo);
                }
                else
                    ghostPots.Add(pozo);
            });

            return ghostPots;
        }


        public long getRounds(RoomModel room)
        {
            return room.roundModel.rounds;
        }

        public static void increaseBlind(RoomModel room)
        {
            room.SMALL_BLIND *= room.BLIND_MULTIPLIER;
            room.BIG_BLIND *= room.BLIND_MULTIPLIER;
        }

        public int getDealerPosition(RoomModel room)
        {
            return room.roundModel.dealerPosition;
        }

        public List<Pot> getPot(RoomModel room)
        {
            return room.roundModel.pots;
        }

        public double getBetOf(int position, RoomModel room)
        {
            return room.roundModel.bets[position];
        }

        public int getStep(RoomModel room)
        {
            return room.roundModel.roundStep;
        }

        public int getWaitingActionFromPlayer(RoomModel room)
        {
            return room.roundModel.waitingActionFromPlayer;
        }

        public int getWaitingTimerFromPlayer(RoomModel room)
        {
            return room.roundModel.waitingTimerFromPlayer;
        }

        public bool checkWaiting(RoomModel room)
        {
            return room.roundModel.isWaiting;
        }

        public static int getBigBlind(RoomModel room)
        {
            return room.BIG_BLIND;
        }

        public static int getSmallBlind(RoomModel room)
        {
            return room.SMALL_BLIND;
        }

        public bool haveCards(int pos, RoomModel room)
        {
            return room.roundModel.playerFirstCards[pos] != null;
        }

        public bool isInGame(int pos, RoomModel room)
        {
            return room.roundModel.usersInGame[pos] != null;
        }

        public Card[] getCommunityCards(RoomModel room)
        {
            if (room.roundModel.roundStep == 2)
                return room.roundModel.flop;

            if (room.roundModel.roundStep == 3)
            {
                //ArrayUtils.AddRange(flop, turn);
                Card[] card = new Card[room.roundModel.flop.Length + 1];


                return UpdateArray(card, room.roundModel.flop, room.roundModel.turn);
            }
            if (room.roundModel.roundStep == 4)
            {
                Card[] card = new Card[room.roundModel.flop.Length + 2];

                return UpdateArrayy(card, room.roundModel.flop, room.roundModel.turn, room.roundModel.river);
            }
            return new Card[] { };
        }

        public Card[] UpdateArray(Card[] newArr, Card[] arr, Card turn)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                newArr[i] = arr[i];
            }
            newArr[arr.Length] = turn;
            return newArr;
        }

        public Card[] UpdateArrayy(Card[] newArr, Card[] arr, Card turn, Card river)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                newArr[i] = arr[i];
            }
            newArr[arr.Length] = turn;
            newArr[arr.Length + 1] = river;
            return newArr;
        }



        public void threadWait(int time)
        {
            // FIXME: fix this:
            try
            {
                System.Threading.Thread.Sleep(time);
            }
            catch (Exception e)
            {
                Console.WriteLine("INTERRUPTED EXCEPTION", e);
            }
        }

        public SchemaCard[] getCards(int pos, RoomModel room)
        {
            if (room.roundModel.playerFirstCards[pos] == null)
            {
                return null;
            }
            return new SchemaCard[] { Utils.getSchemaFromCard(room.roundModel.playerFirstCards[pos]), Utils.getSchemaFromCard(room.roundModel.playerSecondCards[pos]) };
        }



        public bool isAllinAllIn(RoomModel room)
        {
            int usersInGameNotAllIn = 0;
            int userPending = 0;
            double maxBet = 0;
            int maxBeter = 0;
            for (int i = 0; i < room.roundModel.usersInGame.Length; i++)
            {
                if (room.roundModel.usersInGame[i] != null && !room.
                    roundModel.usersInGameDescriptor[i].isAllIn)
                {
                    usersInGameNotAllIn++;
                    userPending = i;
                }
                else if (room.roundModel.usersInGame[i] != null)
                {
                    if (room.roundModel.bets[i] > maxBet)
                    {
                        maxBet = room.roundModel.bets[i];
                        maxBeter = i;
                    }
                }
            }
            if (usersInGameNotAllIn == 0)
            {
                return true;
            }
            if (usersInGameNotAllIn == 1)
            {
                return room.roundModel.bets[userPending] >= room.roundModel.bets[maxBeter];
            }
            return false;
        }



        public void updateChips(RoomModel room)
        {
            ChipStatus cs = new ChipStatus
            {
                status = new List<IndividualChipStatus>()
            };
            for (int i = 0; i < room.roundModel.usersInGame.Length; i++)
                if (room.roundModel.usersInGame[i] != null)
                {
                    IndividualChipStatus ics = new IndividualChipStatus
                    {
                        chips = room.roundModel.usersInGame[i].chips,
                        position = i
                    };
                    cs.status.Add(ics);
                }

            _roomHubContext.Clients.Group(room.roomId.ToString()).SendAsync("/GameController/chipStatus", cs);
        }

        //private static IHubContext<RoomHub> roomHub;

        //public static IHubContext<RoomHub> getSessionHandler()
        //{
        //	return roomHub;
        //}
        //public static void setHubHandler(IHubContext<RoomHub> roomHub)
        //{
        //	RoundGame.roomHub = roomHub;
        //}


        #endregion

        #region Utils
        public void InitGameHandlerLst()
        {
            if (Rooms.Count <= 0)
            {
                List<Room> roomList = _gameLogSvc.GetRoom().Result;
                roomList.ForEach(_ =>
                {
                    var room = new RoomModel()
                    {
                        roomId = _.RoomId,
                        maxPlayers = _.MaxPlayers,
                        Name = _.RoomName,
                        Started = true,
                        minCoinsForAcces = _.MinCoinForAccess,
                        Users = new List<UserData>(),
                        PlayerArray = new UserData[_.MaxPlayers],
                        BIG_BLIND = _.BigBlind,
                        SMALL_BLIND = _.SmallBlind,
                        MAX_RAISE = _.MaxRaise,
                        MIN_RAISE = _.MinRaise,
    
    
                };

                    Rooms.TryAdd(room.roomId.ToString(), room);
                });
            }
        }

        public async Task AddRoundToDB(RoomModel room, ResultSet resultSet, UserData[] newUserArray)
        {
            await _gameLogSvc.CreateRoundLogs(room, resultSet, newUserArray, getCommunityCards(room)).ConfigureAwait(false);
        }

        public static int convertStringtoValue(string number)
        {
            int numberTemp = 0;
            try
            {
                numberTemp = int.Parse(number);
            }
            catch (Exception ex)
            {
                switch (number)
                {
                    case "T": numberTemp = 10; break;
                    case "J": numberTemp = 11; break;
                    case "Q": numberTemp = 12; break;
                    case "K": numberTemp = 13; break;
                    case "A": numberTemp = 14; break;
                }
            }

            return numberTemp;
        }

        public static HandValues GetHandValues(List<Card> tableCards, List<Card> handCards)
        {
            var tableCardsTemp = converCardToString(tableCards);
            var handCalue = converCardToString(handCards);
            Hand player1 = new Hand(handCalue, tableCardsTemp);
            var best5Cards = GetBestFiveCard(tableCardsTemp, handCalue, player1.MaskValue);

            return new HandValues { handName = player1.Description, handPoints = player1.HandValue , playerWinningCards = best5Cards };
        }

        static  List<SchemaCard> GetBestFiveCard(string board, string winner_hand, ulong mask)
        {
            ulong totalmask = Hand.ParseHand(winner_hand + " " + board);
            ulong bestfivemask = BestFiveCards(totalmask);
            var bestfiveString = Hand.MaskToString(bestfivemask);

            return converStringToCard(bestfiveString);
        }

        public static List<SchemaCard> converStringToCard(string best5cards)
        {
            var listTEmp = new List<SchemaCard>();
            string vs = "";
            string[] stringArrray = best5cards.Split(new char[0]);
            foreach (var item in stringArrray)
            {

                var cardts = new SchemaCard(converStringSuitToEnum(item[1].ToString()), convertStringSValuetoValue(item[0].ToString()));
                listTEmp.Add(cardts);
            }
            return listTEmp;
        }



        public static int convertStringSValuetoValue(string number)
        {
            int numberTemp = 0;
            try
            {
                numberTemp = int.Parse(number);
            }
            catch (Exception ex)
            {
                switch (number)
                {
                    case "T": numberTemp = 10; break;
                    case "J": numberTemp = 11; break;
                    case "Q": numberTemp = 12; break;
                    case "K": numberTemp = 13; break;
                    case "A": numberTemp = 14; break;
                }
            }

            return numberTemp;
        }

        public static int converStringSuitToEnum(string suit)
        {
            var suitString = 0;

            switch (suit)
            {
                case "h": suitString = 0; break;
                case "d": suitString = 1; break;
                case "c": suitString = 2; break;
                case "s": suitString = 3; break;
            }
            return suitString;
        }



        public static string converCardToString(List<Card> cards)
        {
            string vs = "";
            for (int i = 0; i < cards.Count; i++)
            {
                if (i == cards.Count - 1)
                {
                    vs = vs + getNameOf(cards[i].value.NumericValue) + converEnumToString(cards[i].suit);
                }
                else
                {
                    vs = vs + getNameOf(cards[i].value.NumericValue) + converEnumToString(cards[i].suit) + " ";
                }


            }
            return vs;
        }

        public static String getNameOf(int number)
        {
            if (number < 11)
            {
                return "" + number;
            }
            else if (number == 11)
            {
                return "j";
            }
            else if (number == 12)
            {
                return "q";
            }
            else if (number == 13)
            {
                return "k";
            }
            return "a";
        }

        public static string converEnumToString(Suit suit)
        {
            var suitString = "";

            switch (suit.ordinal())
            {
                case 0: suitString = "h"; break;
                case 1: suitString = "d"; break;
                case 2: suitString = "c"; break;
                case 3: suitString = "s"; break;
            }
            return suitString;
        }



        private static System.Timers.Timer aTimer;
        private static int TimerIn = 0;
        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += ((o, e) =>
            {
                TimerIn += 1;
                if (TimerIn <= 20)
                {
                    Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff} , Timer: " + TimerIn,
                e.SignalTime);
                }
                else
                {
                    aTimer.Stop();
                    TimerIn = 0;
                    aTimer.Start();
                }
            });
        }

        //private List<UserLogModel> GetUserForRound(RoundModel room, int RoundUsersLogId)
        //{
        //    List<UserLogModel> userLogModels = new List<UserLogModel>();

        //    foreach (var item in room.usersInGame)
        //    {
        //        if (item != null)
        //        userLogModels.Add(new UserLogModel
        //        {
        //            FullName = item.dataBlock.DisplayName,
        //            PlayerId = item.dataBlock.PlayerId,
        //            //RoundUsersLogId = RoundUsersLogId
        //        });
        //    }

        //    return userLogModels;
        //}

        //private List<CardModel> UserCards(RoomModel room)
        //{

        //    for (int i = 0; i < room.roundModel.usersInGame.Length; i++)
        //        if (room.roundModel.usersInGame[i] != null)
        //        {
        //            List<Card> hand = new List<Card>
        //            {
        //                room.roundModel.playerFirstCards[i],
        //                room.roundModel.playerSecondCards[i]
        //            };

        //        }
        //    return cards1;
        //}



        private static ulong BestFiveCards(ulong hand)
        {
#if DEBUG
            if (Hand.BitCount(hand) < 5 || Hand.BitCount(hand) > 7) throw new ArgumentException();
#endif
            uint hv = Hand.Evaluate(hand);

            // Loop through possible 5 card hands
            foreach (ulong hand5 in Hand.Hands(0UL, ~hand, 5))
            {
                if (Hand.Evaluate(hand5) == hv)
                    return hand5;
            }
            throw new Exception("Error");
        }

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
            return firstCards.suit.innerEnumValue.ToString() + firstCards.value.innerEnumValue.ToString() + ',' + secondCard.suit.innerEnumValue.ToString() + secondCard.value.innerEnumValue.ToString();
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
                roundWinners.Add(cardModel);
            }

            return roundWinners;
        }



        private async Task<UserData> GetCurrentUser(int userId, ChallengeActions challengeActions, UserDataStatus userDataStatus)
        {
            var userdb = await _gameLogSvc.GetUserByID(userId);

            UserData userData = new UserData
            {
                userID = userdb.Id,
                lastChallenge = null,
                transactionID = null,
                challengeAction = challengeActions,
                status = userDataStatus,
                chips = (long)userdb.Chips,
                requestForDeposit = 0,
                dataBlock = userdb
            };
            userData.sessID.Add(Context.ConnectionId);
            return userData;
        }


        #endregion

        #region Testing 

        int counter = 5;
        int plays = 0;
        public void TestingProcessDecision(RoomModel room)
        {
            if (plays == 0)
            {
                DecisionInform decisionInform = new DecisionInform
                {
                    action = "call",
                    ammount = 25,
                    position = room.roundModel.lastActionedPosition
                };

                //processDecision(decisionInform, room.roundModel.usersInGame[room.roundModel.waitingActionFromPlayer].dataBlock.PlayerId, room.roundModel.usersInGame[room.roundModel.waitingActionFromPlayer]);
                counter = 5;
                plays = 1;

            }
            else if (plays == 1)
            {
                DecisionInform decisionInform = new DecisionInform
                {
                    action = "check",
                    ammount = 0,
                    position = room.roundModel.lastActionedPosition
                };

                //processDecision(decisionInform, room.roundModel.usersInGame[room.roundModel.waitingActionFromPlayer], room);
                counter = 5;
                plays = 1;

            }
            else if (plays == 2)
            {
                if (counter == 0)
                {
                }
            }
        }
        #endregion

        #region OldCode
        // OLD
        ////TODO - When player sit in room 
        //public void SitPlayerInRoomAsync(string roomId, string userId, int tablePlace)
        //{
        //    var check = (RoomExists: Rooms.TryGetValue(roomId, out var item),
        //           UserIsInRoom: item.Users.Any(u => u.sessID == Context.UserIdentifier && u.challengeAction != ChallengeActions.DEPOSIT),
        //           GameInProgress: item.Started == false);

        //    var result = check switch
        //    {
        //        (true, false, false) => EnterRoomResult.Ok,
        //        (true, false, true) => EnterRoomResult.GameAlreadyInProgress,
        //        (true, true, _) => EnterRoomResult.UserAlreadyInRoom,
        //        (false, _, _) => EnterRoomResult.InvalidRoomId,
        //    };
        //    if (result == EnterRoomResult.Ok)
        //    {
        //        var user = GetCurrentUser(int.Parse(userId), ChallengeActions.DEPOSIT, UserDataStatus.PENDING);

        //        if (user.chips > item.minCoinsForAcces)
        //        {
        //            bool exists = item.Users.Any(_ => _.dataBlock.PlayerId == int.Parse(userId));

        //            sitFlow(tablePlace, user, item);
        //        }
        //        else
        //        {
        //            //await Clients.Client(user.sessID).SendAsync("GameController/deposit", "NoMoney");
        //        }
        //    }

        //}

        #endregion
    }
}