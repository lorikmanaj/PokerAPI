using System.Collections.Generic;

using ModelService.GameModels;
using PokerLogic.Models.SchemaModels.inGame;

namespace PokerLogic.Models.Accesing
{



	public class Snapshot : AccessSchema
	{

		public IList<SnapshotPlayer> players { get; set; }
		public IList<double> pots { get; set; }
		public IList<SchemaCard> communityCards { get; set; }
		public bool isInRest { get; set; }
		public bool isDealing { get; set; }
		public int dealerPosition { get; set; } = -1;
		public int roundStep { get; set; }
		public int myPosition { get; set; } = -1;
		public int waitingFor { get; set; }

		public int waitingForTimer { get; set; }

		public int smallBlind { get; set; }
		public int bigBlind { get; set; }
		public BetDecision betDecision { get; set; }

		public Snapshot() : base("snapshot")
		{
		}

	}

}