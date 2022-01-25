using PokerLogic.Models.SchemaModels;
using System.Collections.Generic;

namespace PokerLogic.Models.SchemaModels.inGame
{



	public class ChipStatus : InGameSchema
	{

		public List<IndividualChipStatus> status { get; set; }

		public ChipStatus() : base("ChipStatus")
		{
		}

	}

}