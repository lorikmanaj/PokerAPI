using System.Collections.Generic;

using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{



	public class Pots : InGameSchema
	{
		public IList<double> pots { get; set; } = new List<double>(); 
		public Pots() : base("Pots")
		{
		}

	}

}