using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class DecisionInform : InGameSchema
	{

		public string action { get; set; }
		public double ammount { get; set; }
		public int position { get; set; }

		public bool isLeaving { get; set; }

		public DecisionInform() : base("decisionInform")
		{
		}
	}

}