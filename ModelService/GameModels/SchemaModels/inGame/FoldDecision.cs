using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class FoldDecision : InGameSchema
	{

		public int position { get; set; }

		public FoldDecision() : base("FoldDecision")
		{
		}


	}

}