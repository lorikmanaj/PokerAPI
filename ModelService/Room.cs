using ModelService.GameLogModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModelService
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string AccessPassword { get; set; }
        public string SecurityToken { get; set; }
		public string ServerIP { get; set; }
		public string GProto { get; set; }
		public int MaxPlayers { get; set; }
		public int MinRaise { get; set; }
		public int MaxRaise { get; set; }
		public int SmallBlind { get; set; }
		public int BigBlind { get; set; }
		public string Description { get; set; }
		public int MinCoinForAccess { get; set; }
		public int MaxCoinForAccess { get; set; }
		public string RecoveryEmail { get; set; }
		public int BadLogins { get; set; }
		public bool NowConnected { get; set; }
		public bool IsOfficial { get; set; }

		public double FeePercentage { get; set; }

		public int PlayersIngame { get; set; }

		public virtual ICollection<RoomRoundLog> RoomRoundLogs { get; set; }
	}
}
