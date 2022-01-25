using PokerLogic.Models.SchemaModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService.GameModels
{
	public class AccessSchema : SchemaTexasHoldem {
	
		public AccessSchema(String schema)
		{
				this.name_space = "access";
				this.schema = schema;
		}

	}
}
