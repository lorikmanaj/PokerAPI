using GameService;
using Microsoft.AspNetCore.SignalR;
using ModelService.RoundGameModel.Deck;
using Newtonsoft.Json;
using PokerLogic.Models;
using PokerLogic.Models.Generic;
using PokerLogic.Models.SchemaModels;
using PokerLogic.Models.SchemaModels.inGame;
using PokerLogic.Utils;
using PokerAPI.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace GameService
{
    public class PokerRoomSvc : IPokerRoomSvc
    {
        //private static /*final*/ Logger log = LoggerFactory.getLogger(PokerRoom.class);
        private UserData[] usersInTable;
        private UserData[] usersInGame;
        private List<RoundGame> rounds = new List<RoundGame>();
        private List<UserData> leaveRequests = new List<UserData>();
        private int tableSize;
        //private SessionHandlerInt sessionHandler;
        private int dealerPosition;
        private RoundGame actualRound;
        //private OrchestratorPipe orchestratorPipe;

        IHubContext<RoomHub> _roomHub;
        IServiceProvider _serviceProvider;
        public bool inGame;

        public void setUsersInTableRef(UserData[] usersInTable, IHubContext<RoomHub> roomHub, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _roomHub = roomHub;
            this.usersInTable = usersInTable;


            this.usersInTable = usersInTable;
            //RoundGame.setHubHandler(roomHub);
            tableSize = usersInTable.Length;
            this.inGame = false;
        }
        public void dumpSnapshot(string sessID, object objectID)
        {
            throw new NotImplementedException();
        }

        public void receivedMessage(SchemaGameProto schemaGameProto, string serializedMessage, string socketSessionID)
        {
            DecisionInform dI = JsonConvert.DeserializeObject<DecisionInform>(serializedMessage);
            throw new NotImplementedException();
        }

        public void onNewPlayerSitdown(UserData player)
        {
            throw new NotImplementedException();
        }

        public void onDeposit(UserData player, long chipsDeposited)
        {

            //log.debug("New Deposit to: " + player.userID + " chips: " + chipsDeposited);
            for (int i = 0; i < usersInTable.Length; i++)
            {
                if (usersInTable[i] != null && usersInTable[i].userID == player.userID)
                {
                    DepositAnnouncement da = new DepositAnnouncement(i, chipsDeposited);
                    //sessionHandler.sendToAll("/GameController/depositAnnouncement", da);
                }
            }

        }

        public void onUserLeave(SchemaGameProto schemaGameProto, string serializedMessage, string socketSessionID)
        {
            throw new NotImplementedException();
        }

        public void setUsersInTableRef(UserData[] usersInTable)
        {
            throw new NotImplementedException();
        }

        public void checkStartGame()
        {
            //log.debug("Check Start Game");

            if (!inGame && Utils.checkPlayers(usersInTable) >= 2)
            { // FIXME: change this, get from configuration, is in two only for test/dev purposes.
              // START GAME
                this.processLeaveRequests();
                //log.debug("START GAME");
                this.inGame = true;
                // start game:
                StartGame startGame = new StartGame();
                // FIXME: put this value in the configuration file.
                startGame.startIn = 20; // initial time
                                        //final SessionHandlerInt _sessionHandler = sessionHandler;
                timer = new System.Timers.Timer();
                timer.Interval = 5000;
                timer.Elapsed += Timer_Elapsed;

                timer.Start();


                //sessionHandler.sendToAll("/GameController/startGame", startGame);
            }


        }

        int timerTick = 5;
        System.Timers.Timer timer;
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerTick -= 1;
            if (timerTick <= 0)
            {
                timer.Stop();
                realStartGame();
            }
            throw new NotImplementedException();
        }



        public void realStartGame()
        {
            //log.debug("Starting game.");
            startRound();
        }


        private void startRound()
        {
            // TODO: ignore players sitted but without deposit in usersInGame:
            this.processLeaveRequests();
            // define the dealer
            this.dealerPosition = Utils.getRandomPositionOfTakens(usersInTable);
            if (Utils.countUsersCanPlay(usersInTable) > 1)
            {
                usersInGame = Utils.getNewArrayOfUsers(usersInTable);
                this.dealerPosition = Utils.getNextPositionOfPlayers(usersInGame, this.dealerPosition);
                rounds.Add(actualRound);
                actualRound = new RoundGame(new Deck(), usersInGame, this.dealerPosition);
                if (actualRound.start())
                {
                    startRound();
                }
            }
            else
            {
                this.inGame = false;
                StartGame startGame = new StartGame();
                startGame.startIn = -1; // initial time
                actualRound = null;
                //sessionHandler.sendToAll("/GameController/startGame", startGame);
                Utils.getPlayersWithoutChips(usersInTable).ForEach(user => {
                    //sessionHandler.sendToSessID("GameController/deposit", user.sessID, new RequestDeposit());
                });
            }
        }

        private void processLeaveRequests()
        {
            this.leaveRequests.ForEach(lr => {
                int ppos = Utils.getPlyerPosition(usersInTable, lr);
                // enviar fichas al orchestrator.
                if (usersInTable[ppos] != null)
                {
                    //var ue = new UserEnd();
                    //ue.refoundCoins = usersInTable[ppos].chips;
                    //ue.userID = usersInTable[ppos].userID;
                    //this.orchestratorPipe.SendToOrchestrator(ue);
                }
                usersInTable[ppos] = null;
                var ln = new LeaveNotify();
                ln.position = ppos;
                //sessionHandler.sendToAll("/GameController/leaveNotify", ln);

            });
            this.leaveRequests.Clear();
        }

        public void setUsersInTableRef(UserData[] usersInTable, IHubContext<RoomHub> _roomHub)
        {

        }
    }
}
