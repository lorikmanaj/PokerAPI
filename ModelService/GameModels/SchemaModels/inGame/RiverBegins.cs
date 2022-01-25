using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class RiverBegins : InGameSchema
	{

		public SchemaCard card { get; set; }

		public RiverBegins() : base("riverBegins")
		{
		}
	}

}