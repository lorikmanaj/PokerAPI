using ModelService.GameModels;

namespace PokerLogic.Models.Accesing
{
	

	public class Announcement : AccessSchema
	{

		public int position { get; set; }
		public string user { get; set; }
		public string avatar { get; set; }
		public double chips { get; set; }
		public long userID { get; set; }

		public Announcement() : base("announcement")
		{
		}

	}

}