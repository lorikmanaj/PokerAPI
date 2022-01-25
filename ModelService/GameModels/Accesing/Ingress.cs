using ModelService.GameModels;

namespace PokerLogic.Models.Accesing
{
	
	public class Ingress : AccessSchema
	{

		public double chips { get; set; }
		public int position { get; set; }

		public Ingress() : base("ingress")
		{
		}

	}

}
