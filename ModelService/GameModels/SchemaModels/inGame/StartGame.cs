using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class StartGame : InGameSchema
	{

		public int startIn { get; set; }

		public StartGame() : base("startGame")
		{
		}

	}

}