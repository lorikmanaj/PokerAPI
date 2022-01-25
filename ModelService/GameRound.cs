using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class GameRound
    {
        [Key]
        public int GameRoundId { get; set; }
        [ForeignKey(nameof(GameInfo))]
        public int GameInfoId { get; set; }
        public int RoundCounter { get; set; }

        public virtual GameInfo GameInfo { get; set; }
        public virtual ICollection<GameRoundAction> GameRoundActions { get; set; }
        public virtual ICollection<ApplicationUser> PlayersInRound { get; set; }
    }
}
