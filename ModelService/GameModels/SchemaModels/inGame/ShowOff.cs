using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{



	public class ShowOff : InGameSchema
	{

		public class PositionCard
		{
			private readonly ShowOff outerInstance;

			public PositionCard(ShowOff outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public SchemaCard first { get; set; }
			public SchemaCard second { get; set; }
		}

		public PositionCard[] positionCards { get; set; }

		public ShowOff(int quantity) : base("showOff")
		{
			positionCards = new PositionCard[quantity];
		}

		public virtual void setCards(int position, SchemaCard first, SchemaCard second)
		{
			PositionCard pc = new PositionCard(this);
			pc.first = first;
			pc.second = second;
			positionCards[position] = pc;
		}

	}

}