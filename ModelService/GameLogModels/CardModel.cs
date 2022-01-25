using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService.GameLogModels
{
    public class CardModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CardId { get; set; }
        public int Suit { get; set; }
        public int Value { get; set; }

        [ForeignKey(nameof(RoundLog))]
        public string RoundLogId { get; set; }

        public virtual RoundLog RoundLog { get; set; }
    }
}
