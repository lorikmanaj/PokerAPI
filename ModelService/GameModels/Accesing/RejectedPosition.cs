using System.Collections.Generic;

using ModelService.GameModels;

namespace PokerLogic.Models.Accesing
{



	public class RejectedPosition : AccessSchema
	{

		public IList<int> positions;

		public RejectedPosition() : base("rejectedPosition")
		{
		}

	}

}