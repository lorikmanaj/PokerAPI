using ModelService.GameModels;
using System.Collections.Generic;

namespace PokerLogic.Models.Accesing
{

	public class DefinePosition : AccessSchema
	{

		public IList<int> positions { get; set; }

		public DefinePosition() : base("definePosition")
		{
		}

	}

}