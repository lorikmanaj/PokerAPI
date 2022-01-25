using ModelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerLogic.Models.Generic
{

	public class UserData
	{

		public string userID { get; set; }
		public List<string> sessID { get; set; } = new List<string>();
		public Challenge lastChallenge { get; set; }
		public string transactionID { get; set; }
		public ChallengeActions challengeAction { get; set; }
		public UserDataStatus status { get; set; }

		public double chips { get; set; } // chips in table
										//TO - DO DB
		public ApplicationUser dataBlock { get; set; } // user of DB.
		public long requestForDeposit { get; set; }

		public int tablePosition { get; set; }
	}
}
