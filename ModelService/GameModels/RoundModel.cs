using ModelService.RoundGameModel.Cards;
using ModelService.RoundGameModel.Deck;
using ModelService.RoundGameModel.Hands;
using PokerLogic;
using PokerLogic.Models.Generic;
using PokerLogic.Models.SchemaModels.inGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService.GameModels
{
    public class RoundModel
    {
        public long rounds = 0;
             // TODO: adjust according to configuration.
        public  System.Timers.Timer TimerOfWaitingPlayer { get; set; } = new System.Timers.Timer();

        public bool isFinishedPlayerDidntPlay { get; set; } = false;
        // player datas:
        //public List<UserData> usersInGame { get; set; } = new List<UserData>();
        public UserData[] usersInGame;
        public UserMetaData[] usersInGameDescriptor { get; set; } // this used for Add info to userdata in this round.
        public double[] bets { get; set; }
        public Card[] playerFirstCards { get; set; }
        public Card[] playerSecondCards { get; set; }
        public Card[] flop { get; set; }
        public Card turn { get; set; }
        public Card river { get; set; }
        public HandValues[] hands { get; set; }
        
        public string bestfiveHands { get; set; }

        public int roundStep { get; set; }
        public int dealerPosition { get; set; } = 0;
        public int waitingActionFromPlayer { get; set; } // id of player are waiting.
        public int waitingTimerFromPlayer { get; set; }
        public int lastActionedPosition { get; set; } // for cut actions.
        public int bigBlind { get; set; }
        public double lastRise { get; set; }
        public List<Pot> pots { get; set; } = new List<Pot>();

        public double FeeForRound { get; set; }

        public Deck deck { get; set; }
        public bool isWaiting { get; set; } = false;

        public int lastActivePositionDetected { get; set; }
        public ResultSet winnersResultSet { get; set; }


    }
}
