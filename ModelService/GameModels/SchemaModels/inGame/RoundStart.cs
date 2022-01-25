using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class RoundStart : InGameSchema
	{

		public int dealerPosition { get; set; }
		public long roundNumber { get; set; }

		public RoundStart() : base("roundStart")
		{
		}

	}

}