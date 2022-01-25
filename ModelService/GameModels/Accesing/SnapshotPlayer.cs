using ModelService.GameModels;
using PokerLogic.Models.SchemaModels.inGame;

namespace PokerLogic.Models.Accesing
{


	public class SnapshotPlayer
	{

		public string nick { get; set; }
		public string photo { get; set; }
		public double chips { get; set; }
		public double actualBet { get; set; }
		public bool showingCards { get; set; }
		public bool haveCards { get; set; }
		public bool inGame { get; set; }
		public long userID { get; set; }
		public SchemaCard[] cards { get; set; }

	}

}