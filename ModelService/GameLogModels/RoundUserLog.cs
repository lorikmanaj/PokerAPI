using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService.GameLogModels
{
    public class RoundUserLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoundUsersLogId { get; set; }

        [ForeignKey(nameof(RoundLog))]
        public string RoundLogId { get; set; }

        //[ForeignKey(nameof(UserLog))]
        //public int UserLogId { get; set; }
        public int PlayerId { get; set; }
        public string FullName { get; set; }
        public string RoundUserCardsJson { get; set; }
        public int UserPoints { get; set; }

        //public virtual UserLogModel UserLog { get; set; }
        public virtual RoundLog RoundLog { get; set; }
        //public virtual ICollection<UserLogModel> RoundUsers { get; set; }
    }
}
