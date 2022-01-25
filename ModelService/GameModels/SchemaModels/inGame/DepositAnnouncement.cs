using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class DepositAnnouncement : InGameSchema
	{

		public int position { get; set; }
		public long quantity { get; set; }

		public DepositAnnouncement() : base("DepositAnnouncement")
		{
		}

		public DepositAnnouncement(int position, long quantity) : base("DepositAnnouncement")
		{
			this.position = position;
			this.quantity = quantity;
		}

	}

}