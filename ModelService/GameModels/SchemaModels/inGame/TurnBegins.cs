using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{
	

	public class TurnBegins : InGameSchema
	{

		public SchemaCard card { get; set; }

		public TurnBegins() : base("turnBegins")
		{
		}
	}

}