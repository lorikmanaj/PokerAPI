using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService.GameLogModels
{
    public class RoundWinner
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int RoundWinnerLogId { get; set; }
		[ForeignKey(nameof(RoundLog))]
		public string RoundLogId { get; set; }
		public int PlayerId { get; set; }
		public double Pot { get; set; }
		public String Reason { get; set; }
		public long Points { get; set; }
		public long SecondaryPoints { get; set; }
		public double FullPot { get; set; }
		public int PotNumber { get; set; }
	
		public double GameFeeCharge { get; set; }

		public virtual RoundLog RoundLog { get; set; }
	}
}
