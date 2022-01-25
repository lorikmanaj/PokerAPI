using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class Ban
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int BannId { get; set; }
		[ForeignKey(nameof(BanningAdmin))]
		public string AdUserId { get; set; }
		[ForeignKey(nameof(BannedUser))]
		public string UserId { get; set; }
		public string Reason { get; set; }
		public DateTime RegisteredBanDate { get; set; }
		public DateTime ExpireBanDate { get; set; }
		public long Restarts;

		public virtual ApplicationUser BannedUser { get; set; }
		public virtual ApplicationUser BanningAdmin { get; set; }
	}
}
