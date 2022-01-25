using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class UserInRoom
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        [ForeignKey(nameof(Room))]
        public int RoomId { get; set; }
        public DateTime Registered { get; set; }
        public int Position { get; set; }
        public decimal MoneyInRoom { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Room Room { get; set; }
    }
}
