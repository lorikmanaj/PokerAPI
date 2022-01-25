using ModelService.RoundGameModel.Cards;
using ModelService.RoundGameModel.Hands;
using PokerLogic.Models.SchemaModels.inGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService.RoundGameModel.Hands
{
    public class HandValues
    {
		public uint handPoints;
		public int secondaryHandPoint;
		public List<int> kickerPoint;
		public HandType type;
		public String handName;
		public String kickerName;

		public List<SchemaCard> playerWinningCards;
	}
}
