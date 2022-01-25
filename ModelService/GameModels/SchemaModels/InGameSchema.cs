using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerLogic.Models.SchemaModels
{
    public class InGameSchema : SchemaTexasHoldem {

		public InGameSchema(string schema)
		{

			this.name_space = "inGame";
			this.schema = schema;
		}

	}
}
