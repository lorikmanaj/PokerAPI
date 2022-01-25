using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService.GameLogModels
{
    public class RoomRoundLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomRoundLogId { get; set; }
        [ForeignKey(nameof(Room))]
        public int RoomId { get; set; }
        [ForeignKey(nameof(RoundLog))]
        public string RoundLogId { get; set; }
    
        public virtual Room Room { get; set; }
        public virtual RoundLog RoundLog { get; set; }
    }
}
