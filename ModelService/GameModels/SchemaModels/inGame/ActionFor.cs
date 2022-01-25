using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{

	public class ActionFor : InGameSchema
	{

		public int position { get; set; }
		public int remainingTime { get; set; }

		public ActionFor() : base("actionFor")
		{
		}
	}

}