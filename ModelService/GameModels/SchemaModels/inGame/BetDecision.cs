using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class BetDecision : InGameSchema
	{

		public double toCall { get; set; }
		public bool canCheck { get; set; }
		public long minRaise { get; set; }
		public long maxRaise { get; set; }

		public BetDecision() : base("betDecision")
		{
		}

	}

}