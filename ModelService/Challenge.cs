using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class Challenge
    {
        [Key]
        public int ChallengeId { get; set; }
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        [ForeignKey(nameof(Room))]
        public int RoomId { get; set; }
        public string ChallengeDescription { get; set; }
        public decimal Deposit { get; set; }
    
        public virtual ApplicationUser User { get; set; }
        public virtual Room Room { get; set; }
    }
}
