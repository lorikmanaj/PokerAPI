using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{

	public class LeaveNotify : InGameSchema
	{

		public int position { get; set; }

		public LeaveNotify() : base("leaveNotify")
		{
		}

	}

}