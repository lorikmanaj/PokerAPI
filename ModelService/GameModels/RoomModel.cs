using PokerLogic.Models.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService.GameModels
{
    public class RoomModel
    {
        public RoomModel()
        {
            //roundModel = new RoundModel();
            //Users = new List<UserData>();
        }
        public int roomId { get; set; }
        public int maxPlayers { get; set; }
        
        public string Name { get; set; }
        public bool Started { get; set; }
        public bool inGame { get; set; }
        public int minCoinsForAcces { get; set; }

        public List<UserData> Users { get; set; }

        public UserData[] PlayerArray { get; set; }

        public List<UserData> leaveRequests = new List<UserData>();

        public RoundModel roundModel { get; set; }

        public DateTime? timeToStart { get; set; }

        public int RoundStart { get; set; } = 10;

        public int MIN_RAISE = 50; // TODO: adjust according to configuration.
        public int MAX_RAISE = -1; // TODO: adjust according to configuration.
        public int SMALL_BLIND = 25; // TODO: adjust according to configuration.
        public int BIG_BLIND = 50; // TODO: adjust according to configuration.
        public int BLIND_MULTIPLIER = 2;
    }
}
