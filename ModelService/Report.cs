using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }
        [ForeignKey(nameof(ReportingUser))]
        public string ReportingUserId { get; set; }
        [ForeignKey(nameof(ReportedUser))]
        public string ReportedUserId { get; set; }

        [ForeignKey(nameof(LastReporter))]
        public string LastReporterId { get; set; }
        public DateTime ReportDate { get; set; }
        public string Message { get; set; }

        public virtual ApplicationUser ReportingUser { get; set; }
        public virtual ApplicationUser ReportedUser { get; set; }
        public virtual ApplicationUser LastReporter { get; set; }
    }
}
