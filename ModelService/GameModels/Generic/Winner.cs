using ModelService.RoundGameModel.Cards;
using PokerLogic.Models.SchemaModels.inGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerLogic.Models.Generic
{
    public class Winner
    {
		public int position { get; set; }
		public double pot { get; set; }
		public String reason { get; set; }
		public long points { get; set; }
		public long secondaryPoints { get; set; }
		public double fullPot { get; set; }
		public int potNumber { get; set; }
		public List<SchemaCard> playerWinningCards { get; set; }
		public double GameFeeCharge { get; set; }
	}
}
