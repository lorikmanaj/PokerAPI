using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class SnapshotRequest : InGameSchema
	{

		public int round { get; set; }

		public SnapshotRequest() : base("SnapshotRequest")
		{
		}

	}

}