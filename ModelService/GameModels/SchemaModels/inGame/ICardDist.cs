using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class ICardDist : InGameSchema
	{

		public int position { get; set; }
		public SchemaCard[] cards { get; set; }

		public ICardDist() : base("iCardDist")
		{
		}
	}

}