using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class GameRoundAction
    {
        [Key]
        public int GameRoundActionId { get; set; }
        [ForeignKey(nameof(GameRound))]
        public int GameRoundId { get; set; }
        [ForeignKey(nameof(Player))]
        public int UserId { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }

        public virtual ApplicationUser Player { get; set; }
        public virtual GameRound GameRound { get; set; } 
    }
}
