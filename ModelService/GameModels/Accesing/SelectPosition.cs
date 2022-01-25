using ModelService.GameModels;

namespace PokerLogic.Models.Accesing
{


	public class SelectPosition : AccessSchema
	{

		public int position;

		public SelectPosition() : base("selectPosition")
		{
		}

	}

}