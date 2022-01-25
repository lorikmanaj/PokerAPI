using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class Blind : InGameSchema
	{

		public double bbChips { get; set; }
		public double sbChips { get; set; }
		public int bbPosition { get; set; }
		public int sbPosition { get; set; }

		public Blind() : base("blind")
		{
		}

	}

}