using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{
	public class SchemaCard
	{
		public int suit { get; set; }
		public int value { get; set; }
		public bool winnerCard { get; set; } = false;
		public SchemaCard(int suit, int value)
		{
			this.suit = suit;
			this.value = value;
		}
	}

}