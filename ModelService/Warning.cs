using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class Warning
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarningId { get; set; }
        [ForeignKey(nameof(ReportedUser))]
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime Registered { get; set; }
        public int Restarts { get; set; }
        [ForeignKey(nameof(LastReporter))]
        public string ReporterId { get; set; }

        public virtual ApplicationUser ReportedUser { get; set; }
        public virtual ApplicationUser LastReporter { get; set; }
    }
}
