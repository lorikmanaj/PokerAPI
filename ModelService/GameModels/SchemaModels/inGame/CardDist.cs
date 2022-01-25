using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{
	

	public class CardDist : InGameSchema
	{

		public int position { get; set; }
		public bool[] cards { get; set; }

		public CardDist() : base("cardDist")
		{
		}
	}

}