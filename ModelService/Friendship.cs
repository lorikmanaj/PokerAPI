using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class Friendship
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FriendShipId { get; set; }
        [ForeignKey(nameof(OriginUser))]
        public string InvitingUser { get; set; }
        [ForeignKey(nameof(TargetUser))]
        public string InvitedUser { get; set; }
        public DateTime RequestedDate { get; set; }
        public bool Accepted { get; set; }

        public virtual ApplicationUser OriginUser { get; set; }
        public virtual ApplicationUser TargetUser { get; set; }
    }
}
