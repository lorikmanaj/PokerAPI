using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModelService
{
    public class GameInfo
    {
        [Key]
        public int GameInfoId { get; set; }
        public DateTime OccurringDate { get; set; }
        public int InitialRoundNrPlayers { get; set; }

        public virtual ICollection<GameRound> GameRounds { get; set; }
        public virtual ICollection<ApplicationUser> PlayersInGame { get; set; }
    }
}
