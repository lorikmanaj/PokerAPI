  using PokerLogic.Models.SchemaModels;

namespace PokerLogic.Models.SchemaModels.inGame
{


	public class ChatMessage : InGameSchema
	{

		public string message { get; set; }
		public string author { get; set; }

		public ChatMessage() : base("ChatMessage")
		{
		}

	}

}