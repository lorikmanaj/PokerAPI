using ModelService.RoundGameModel.Deck;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService.GameLogModels
{
    public class RoundLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string RoundLogId { get; set; }
        public DateTime RoungLogStartDate { get; set; }
        public DateTime RoungLogEndDate { get; set; }
        
        public virtual ICollection<CardModel> RoundCardsJson { get; set; }
        public virtual ICollection<RoundWinner> RoundWinners { get; set; }
    }
}
