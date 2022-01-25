using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class FlopBegins : InGameSchema
	{
		public SchemaCard[] cards { get; set; }
		public FlopBegins() : base("flopBegins")
		{
		}
	}

}