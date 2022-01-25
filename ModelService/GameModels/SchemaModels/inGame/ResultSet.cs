using System.Collections.Generic;
using PokerLogic.Models.Generic;
using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{



	public class ResultSet : InGameSchema
	{

		public IList<Winner> winners { get; set; }

		public ResultSet() : base("resultSet")
		{
		}

	}

}