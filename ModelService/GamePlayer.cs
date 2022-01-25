using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class GamePlayer
    {
        [ForeignKey(nameof(GameInfo))]
        public int GameInfoId { get; set; }
        [ForeignKey(nameof(Player))]
        public string UserId { get; set; }

        public virtual ApplicationUser Player { get; set; }
    }
}
