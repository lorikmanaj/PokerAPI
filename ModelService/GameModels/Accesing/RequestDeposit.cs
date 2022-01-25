using ModelService.GameModels;

namespace PokerLogic.Models.Accesing
{


	public class RequestDeposit : AccessSchema
	{
		public int position { get; set; }
		public int id { get; set; }
		public RequestDeposit() : base("requestDeposit")
		{

		}

	}

}